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

        public CABController(ICABAdminService cabAdminService, UserManager<UKMCABUser> userManager
            )
        {
            _cabAdminService = cabAdminService;
            _userManager = userManager;
        }

        [HttpGet]
        [Route("admin/cab/details/{id?}")]
        public async Task<IActionResult> Details(string id)
        {
            return View(new CABDetailsViewModel());
        }

        [HttpPost]
        [Route("admin/cab/details/{id?}")]
        public async Task<IActionResult> Details(string id, CABDetailsViewModel model, string submitType)
        {
            var appointmentDate = CheckDate(model.AppointmentDate, nameof(model.AppointmentDate), "appointment");
            var renewalDate = CheckDate(model.RenewalDate, nameof(model.RenewalDate), "renewal");
            if (ModelState.IsValid)
            {
                var document = new Document
                {
                    Name = model.Name,
                    CABNumber = model.CABNumber,
                    AppointmentDate = appointmentDate,
                    RenewalDate = renewalDate,
                    UKASReference = model.UKASReference
                };
                if (await _cabAdminService.DocumentWithKeyIdentifiersExistsAsync(document))
                {
                    ModelState.AddModelError(nameof(model.Name), "A document already exists for this CAB name, number or UKAS reference");
                }
                else
                {
                    var user = await _userManager.GetUserAsync(User);
                    var createdDocument = await _cabAdminService.CreateDocumentAsync(user.Email, document);
                    if (createdDocument == null)
                    {
                        ModelState.AddModelError(string.Empty, "Failed to create the document, please try again.");
                    }
                    else if (submitType == "continue")
                    {
                        return RedirectToAction("Contact", "CAB", new { Area = "admin", id= createdDocument.CABId });
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
        public async Task<IActionResult> Contact(string id)
        {
            var documents = await _cabAdminService.FindAllDocumentsByCABIdAsync(id);
            if (!documents.Any(d => d.IsLatest)) // Implies no document or archived
            {
                return RedirectToAction("Index", "Admin", new { Area = "admin" });
            }
            // Pre-populate model for edit
            return View(new CABContactViewModel {CABId = id});
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
                    "Enter a valid email address or phone number.");
            }

            if (ModelState.IsValid)
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
                if (submitType == "continue")
                {
                    return RedirectToAction("BodyDetails", "CAB", new { Area = "admin", id = latestDocument.CABId });
                }
                else
                {
                    return SaveDraft(latestDocument);
                }
            }

            return View(new CABContactViewModel());
        }

        [HttpGet]
        [Route("admin/cab/body-details/{id}")]
        public async Task<IActionResult> BodyDetails(string id)
        {
            var documents = await _cabAdminService.FindAllDocumentsByCABIdAsync(id);
            if (!documents.Any(d => d.IsLatest)) // Implies no document or archived
            {
                return RedirectToAction("Index", "Admin", new { Area = "admin" });
            }
            // Pre-populate model for edit
            return View(new CABBodyDetailsViewModel());
        }







        [HttpGet]
        [Route("admin/cab/cancel/{id}")]
        public async Task<IActionResult> Cancel(string id)
        {
            await _cabAdminService.DeleteDraftDocumentAsync(id);
            return RedirectToAction("Index", "Admin", new { Area = "admin" });
        }







        [HttpGet]
        [Route("admin/cab/create")]
        public async Task<IActionResult> Create()
        {
            var model = new CreateCABViewModel();
            await UpdateModel(model);

            return View(model);
        }

        private async Task UpdateModel(CreateCABViewModel model)
        {
            model.CountryList = Constants.Lists.Countries;
            model.BodyTypeList = Constants.Lists.BodyTypes;
            model.RegulationList = Constants.Lists.LegislativeAreas;
            var user = await _userManager.GetUserAsync(User);
            model.IsUKASUser = await _userManager.IsInRoleAsync(user, Constants.Roles.UKASUser);
        }

        [HttpPost]
        [Route("admin/cab/create")]
        public async Task<IActionResult> Create(State SaveButton, CreateCABViewModel model)
        {
            UpdateModel(model);

            if (model.Regulations == null || !model.Regulations.Any())
            {
                ModelState.AddModelError("Regulations", "Please select at least one regulation from the list.");
            }

            if (string.IsNullOrWhiteSpace(model.Website) &&
                string.IsNullOrWhiteSpace(model.Email) &&
                string.IsNullOrWhiteSpace(model.Phone))
            {
                ModelState.AddModelError("Website",
                    "At least one of Email, Phone, or Website needs to be completed to create a new CAB.");
            }

            if (model.IsUKASUser && string.IsNullOrWhiteSpace(model.UKASReference))
            {
                ModelState.AddModelError("UKASReference", "A valid UKAS reference is required.");
            }
            else if (model.IsUKASUser)
            {
                var existingUKASReferenceDocuments =
                    await _cabAdminService.FindCABDocumentsByUKASReferenceAsync(model.UKASReference);
                if (existingUKASReferenceDocuments.Any())
                {
                    ModelState.AddModelError("UKASReference", "A CAB with this UKAS reference already exists.");
                }
            }

            if (ModelState.IsValid)
            {
                var existingCABDocuments = await _cabAdminService.FindCABDocumentsByNameAsync(model.Name);
                if (existingCABDocuments.Any())
                {
                    ModelState.AddModelError("Name", "A CAB with this name already exists.");
                }
                else
                {
                    var user = await _userManager.GetUserAsync(User);

                    var document = await _cabAdminService.CreateDocumentAsync(user.Email, new Document()
                    {
                        Name = model.Name,
                        UKASReference = model.UKASReference,
                        Address = model.Address,
                        Website = model.Website ?? string.Empty,
                        Email = model.Email ?? string.Empty,
                        Phone = model.Phone ?? string.Empty,
                        BodyTypes = model.BodyTypes != null && model.BodyTypes.Any()
                            ? model.BodyTypes
                            : new List<string>(),
                        LegislativeAreas = model.Regulations
                    });
                    if (document != null)
                    {
                        return RedirectToAction("SchedulesUpload", "FileUpload", new { id = document.CABData.CABId });
                    }

                    ModelState.AddModelError("Name", "Failed to save the CAB to the database. Please try again.");
                }
            }

            return View(model);
        }

    }
}
