using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using UKMCAB.Core.Models;
using UKMCAB.Core.Services;
using UKMCAB.Identity.Stores.CosmosDB;
using UKMCAB.Web.UI.Models.ViewModels.Admin;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers
{
    [Area("admin")]
    [Authorize(Roles = $"{Constants.Roles.OPSSAdmin}")]
    public class CABController : Controller
    {
        private readonly ICABAdminService _cabAdminService;
        private readonly UserManager<UKMCABUser> _userManager;

        public CABController(ICABAdminService cabAdminService, UserManager<UKMCABUser> userManager)
        {
            _cabAdminService = cabAdminService;
            _userManager = userManager;
        }

        [HttpGet]
        [Route("admin/cab/details/{id}")]
        public async Task<IActionResult> Details(string id, bool fromSummary)
        {
            var model = new CABDetailsViewModel();
            if (!id.Equals("create", StringComparison.InvariantCultureIgnoreCase))
            {
                var latestDocument = await GetLatestDocument(id);
                if (latestDocument == null) // Implies no document or archived
                {
                    return RedirectToAction("Index", "Admin", new { Area = "admin" });
                }

                model = new CABDetailsViewModel(latestDocument);
            }

            model.IsFromSummary = fromSummary;
            return View(model);
        }

        private async Task<Document?> GetLatestDocument(string cabId)
        {
            var documents = await _cabAdminService.FindAllDocumentsByCABIdAsync(cabId);
            if (!documents.Any(d => d.IsLatest)) // Implies no document or archived
            {
                return null;
            }

            return documents.Single(d => d.IsLatest);
        }

        [HttpPost]
        [Route("admin/cab/details/{id}")]
        public async Task<IActionResult> Details(string id, CABDetailsViewModel model, string submitType)
        {
            var appointmentDate = CheckDate(model.AppointmentDate, nameof(model.AppointmentDate), "appointment");
            var renewalDate = CheckDate(model.RenewalDate, nameof(model.RenewalDate), "renewal");
            if (ModelState.IsValid)
            {
                var document = await GetLatestDocument(id) ?? new Document();
                document.Name = model.Name;
                document.CABNumber = model.CABNumber;
                document.AppointmentDate = appointmentDate;
                document.RenewalDate = renewalDate;
                document.UKASReference = model.UKASReference;

                if (await _cabAdminService.DocumentWithKeyIdentifiersExistsAsync(document))
                {
                    ModelState.AddModelError(nameof(model.Name), "A document already exists for this CAB name, number or UKAS reference");
                }
                else
                {
                    var user = await _userManager.GetUserAsync(User);
                    var createdDocument = model.IsFromSummary ?
                        await _cabAdminService.UpdateOrCreateDraftDocumentAsync(user.Email, document) :
                        await _cabAdminService.CreateDocumentAsync(user.Email, document);
                    if (createdDocument == null)
                    {
                        ModelState.AddModelError(string.Empty, "Failed to create the document, please try again.");
                    }
                    else if (submitType == Constants.SubmitType.Continue)
                    {
                        return model.IsFromSummary ?
                            RedirectToAction("Summary", "CAB", new { Area = "admin", id= createdDocument.CABId }):
                            RedirectToAction("Contact", "CAB", new { Area = "admin", id= createdDocument.CABId });
                    }
                    else
                    {
                        return SaveDraft(document);
                    }
                }
            }

            return View(new CABDetailsViewModel());
        }

        private DateTime? CheckDate(string date, string modelKey, string errorMessagePart)
        {
            if (DateTime.TryParse(date, out DateTime dateTime))
            {
                return dateTime;
            }
            if(!date.Equals("//"))
            {
                ModelState.AddModelError(modelKey, $"The {errorMessagePart} date is not in a valid date format");
            }
            return null;
        }

        private IActionResult SaveDraft(Document document)
        {
            TempData[Constants.TempDraftKey] = $"Draft record saved for {document.Name} (CAB number {document.CABNumber})";
            return RedirectToAction("Index", "Admin", new { Area = "admin" });
        }

        [HttpGet]
        [Route("admin/cab/contact/{id}")]
        public async Task<IActionResult> Contact(string id, bool fromSummary)
        {
            var documents = await _cabAdminService.FindAllDocumentsByCABIdAsync(id);
            if (!documents.Any(d => d.IsLatest)) // Implies no document or archived
            {
                return RedirectToAction("Index", "Admin", new { Area = "admin" });
            }
            // Pre-populate model for edit
            var latest = documents.Single(d => d.IsLatest);
            var model = new CABContactViewModel(latest);
            model.IsFromSummary = fromSummary;
            return View(model);
        }

        [HttpPost]
        [Route("admin/cab/contact/{id}")]
        public async Task<IActionResult> Contact(string id, CABContactViewModel model, string submitType)
        {
            var documents = await _cabAdminService.FindAllDocumentsByCABIdAsync(id);
            if (!documents.Any(d => d.IsLatest))
            {
                return RedirectToAction("Index", "Admin", new { Area = "admin" });
            }
            var latestDocument = documents.Single(d => d.IsLatest);

            if (string.IsNullOrWhiteSpace(model.Email) &&
                string.IsNullOrWhiteSpace(model.Phone))
            {
                ModelState.AddModelError("Email",
                    "Enter either an email or phone");
            }

            if (ModelState.IsValid || submitType == Constants.SubmitType.Save)
            {
                latestDocument.AddressLine1 = model.AddressLine1;
                latestDocument.AddressLine2 = model.AddressLine2;
                latestDocument.TownCity = model.TownCity;
                latestDocument.Postcode = model.Postcode;
                latestDocument.Country = model.Country;
                latestDocument.Website = model.Website;
                latestDocument.Email = model.Email;
                latestDocument.Phone = model.Phone;
                latestDocument.PointOfContactName = model.PointOfContactName;
                latestDocument.PointOfContactEmail = model.PointOfContactEmail;
                latestDocument.PointOfContactPhone = model.PointOfContactPhone;
                latestDocument.IsPointOfContactPublicDisplay = model.IsPointOfContactPublicDisplay;
                latestDocument.RegisteredOfficeLocation = model.RegisteredOfficeLocation;

                var user = await _userManager.GetUserAsync(User);
                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(user.Email, latestDocument);
                if (submitType == Constants.SubmitType.Continue)
                {
                    return model.IsFromSummary ?
                        RedirectToAction("Summary", "CAB", new { Area = "admin", id = latestDocument.CABId }):
                        RedirectToAction("BodyDetails", "CAB", new { Area = "admin", id = latestDocument.CABId });
                }
                return SaveDraft(latestDocument);
            }

            return View(new CABContactViewModel());
        }

        [HttpGet]
        [Route("admin/cab/body-details/{id}")]
        public async Task<IActionResult> BodyDetails(string id, bool fromSummary)
        {
            var documents = await _cabAdminService.FindAllDocumentsByCABIdAsync(id);
            if (!documents.Any(d => d.IsLatest)) // Implies no document or archived
            {
                return RedirectToAction("Index", "Admin", new { Area = "admin" });
            }
            
            // Pre-populate model for edit
            var latest = documents.Single(d => d.IsLatest);
            var model = new CABBodyDetailsViewModel(latest);
            if (!model.TestingLocations.Any())
            {
                model.TestingLocations.Add(string.Empty);
            }
            model.IsFromSummary = fromSummary;
            return View(model);
        }

        [HttpPost]
        [Route("admin/cab/body-details/{id}")]
        public async Task<IActionResult> BodyDetails(string id, CABBodyDetailsViewModel model, string submitType)
        {
            var documents = await _cabAdminService.FindAllDocumentsByCABIdAsync(id);
            if (!documents.Any(d => d.IsLatest))
            {
                return RedirectToAction("Index", "Admin", new { Area = "admin" });
            }
            var latestDocument = documents.Single(d => d.IsLatest);

            model.TestingLocations = model.TestingLocations != null ? model.TestingLocations.Where(t => !string.IsNullOrWhiteSpace(t)).ToList() : new List<string>();
            if (!model.TestingLocations.Any())
            {
                ModelState.AddModelError("TestingLocations", "Select a registered test location");
                model.TestingLocations.Add(string.Empty);
            }

            if (ModelState.IsValid || submitType == Constants.SubmitType.Save)
            {
                latestDocument.TestingLocations = model.TestingLocations;
                latestDocument.BodyTypes = model.BodyTypes;
                latestDocument.LegislativeAreas = model.LegislativeAreas;
                var user = await _userManager.GetUserAsync(User);

                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(user.Email, latestDocument);
                if (submitType == Constants.SubmitType.Continue)
                { 
                    return model.IsFromSummary ?
                        RedirectToAction("Summary", "CAB", new { Area = "admin", id = latestDocument.CABId }) : 
                        RedirectToAction("SchedulesUpload", "FileUpload", new { Area = "admin", id = latestDocument.CABId });
                }
                return SaveDraft(latestDocument);
            }

            return View(model);
        }

        [HttpGet]
        [Route("admin/cab/summary/{id}")]
        public async Task<IActionResult> Summary(string id)
        {
            var documents = await _cabAdminService.FindAllDocumentsByCABIdAsync(id);
            if (!documents.Any(d => d.IsLatest)) // Implies no document or archived
            {
                return RedirectToAction("Index", "Admin", new { Area = "admin" });
            }

            // Pre-populate model for edit
            var latest = documents.Single(d => d.IsLatest);
            var model = new CABSummaryViewModel
            {
                CABId = latest.CABId,
                CabDetailsViewModel = new CABDetailsViewModel(latest),
                CabContactViewModel = new CABContactViewModel(latest),
                CabBodyDetailsViewModel = new CABBodyDetailsViewModel(latest),
                Schedules = latest.Schedules ?? new List<FileUpload>(),
                Documents = latest.Documents ?? new List<FileUpload>()
            };
            model.ValidCAB = TryValidateModel(model);
            return View(model);
        }

        [HttpPost]
        [Route("admin/cab/summary/{id}")]
        public async Task<IActionResult> Summary(CABSummaryViewModel model)
        {
            var documents = await _cabAdminService.FindAllDocumentsByCABIdAsync(model.CABId);
            if (!documents.Any(d => d.IsLatest)) // Implies no document or archived
            {
                return RedirectToAction("Index", "Admin", new { Area = "admin" });
            }

            // Pre-populate model for edit
            var latest = documents.Single(d => d.IsLatest);
            model.CabDetailsViewModel = new CABDetailsViewModel(latest);
            model.CabContactViewModel = new CABContactViewModel(latest);
            model.CabBodyDetailsViewModel = new CABBodyDetailsViewModel(latest);
            model.Schedules = latest.Schedules ?? new List<FileUpload>();
            model.Documents = latest.Documents ?? new List<FileUpload>();
            model.ValidCAB = TryValidateModel(model);
            if (model.ValidCAB)
            {
                var user = await _userManager.GetUserAsync(User);
                var pubishedDoc = await _cabAdminService.PublishDocumentAsync(user.Email, latest);
                return RedirectToAction("Confirmation", "CAB", new { Area = "admin", id = latest.CABId });
            }

            model.ShowError = true;
            model.ValidCAB = TryValidateModel(model);
            return View(model);
        }

        [HttpGet]
        [Route("admin/cab/confirmation/{id}")]
        public async Task<IActionResult> Confirmation(string id)
        {
            return View();
        }

        [HttpGet]
        [Route("admin/cab/cancel/{id}")]
        public async Task<IActionResult> Cancel(string id)
        {
            await _cabAdminService.DeleteDraftDocumentAsync(id);
            return RedirectToAction("Index", "Admin", new { Area = "admin" });
        }
    }
}
