using Microsoft.AspNetCore.Authorization;
using System.Net;
using System.Security.Claims;
using UKMCAB.Core.Services;
using UKMCAB.Core.Services.Users;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Helpers;
using UKMCAB.Web.UI.Models.ViewModels.Admin;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers
{
    [Area("admin"), Authorize]
    public class CABController : Controller
    {
        private readonly ICABAdminService _cabAdminService;
        private readonly IUserService _userService;

        public CABController(ICABAdminService cabAdminService, IUserService userService)
        {
            _cabAdminService = cabAdminService;
            _userService = userService;
        }

        [HttpGet]
        [Route("admin/cab/about/{id}")]
        public async Task<IActionResult> About(string id, bool fromSummary)
        {
            var model = new CABDetailsViewModel();
            if (!id.Equals("create", StringComparison.InvariantCultureIgnoreCase))
            {
                var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id);
                if (latestDocument == null) // Implies no document or archived
                {
                    return RedirectToAction("CABManagement", "Admin", new { Area = "admin" });
                }

                model = new CABDetailsViewModel(latestDocument);
            }

            model.IsFromSummary = fromSummary;
            return View(model);
        }

        [HttpPost]
        [Route("admin/cab/about/{id}")]
        public async Task<IActionResult> About(string id, CABDetailsViewModel model, string submitType)
        {
            var appointmentDate = DateUtils.CheckDate(ModelState, model.AppointmentDateDay, model.AppointmentDateMonth, model.AppointmentDateYear, nameof(model.AppointmentDate), "appointment");
            var reviewDate = DateUtils.CheckDate(ModelState, model.ReviewDateDay, model.ReviewDateMonth, model.ReviewDateYear, nameof(model.ReviewDate), "review", appointmentDate);

            var document = await _cabAdminService.GetLatestDocumentAsync(id);

            if (submitType == Constants.SubmitType.Add18)
            {
                var createdAudit = document?.AuditLog?.FirstOrDefault(al => al.Status == AuditStatus.Created);
                var publishedAudit = document?.AuditLog?.FirstOrDefault(al => al.Status == AuditStatus.Published);
                var autoRenewDate = document != null && createdAudit != null && publishedAudit == null ? createdAudit.DateTime.AddMonths(18) : DateTime.UtcNow.AddMonths(18);

                autoRenewDate = autoRenewDate.Date < DateTime.Today ? DateTime.UtcNow.AddMonths(1) : autoRenewDate;

                model.ReviewDateDay = autoRenewDate.Day.ToString();
                model.ReviewDateMonth = autoRenewDate.Month.ToString();
                model.ReviewDateYear = autoRenewDate.Year.ToString();
                ModelState.Clear();
            }
            else if (ModelState.IsValid)
            {
                if (document == null)
                {
                    document = new Document()
                    {
                        AuditLog = new List<Audit>()
                    };
                }

                if (string.IsNullOrWhiteSpace(document.URLSlug) || !document.Name.Equals(model.Name))
                {
                    var slug = Slug.Make(model.Name);
                    var newSlug = slug;
                    var existingDocs = await _cabAdminService.FindAllDocumentsByCABURLAsync(newSlug);
                    var index = 0;
                    while (existingDocs.Any(d => !d.CABId.Equals(document.CABId)))
                    {
                        newSlug = $"{slug}-{index++}";
                        existingDocs = await _cabAdminService.FindAllDocumentsByCABURLAsync(newSlug);
                    }

                    document.URLSlug = newSlug;
                }
                document.Name = model.Name;
                document.CABNumber = model.CABNumber;
                document.AppointmentDate = appointmentDate;
                document.RenewalDate = reviewDate;
                document.UKASReference = model.UKASReference;

                var duplicateDocuments = await _cabAdminService.DocumentWithKeyIdentifiersExistsAsync(document);
                if (duplicateDocuments.Any())
                {
                    if (duplicateDocuments.Any(d => d.CABNumber.Equals(model.CABNumber, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        ModelState.AddModelError(nameof(model.CABNumber), "This CAB number already exists\r\n\r\n");
                    }
                    if (duplicateDocuments.Any(d => d.UKASReference != null && d.UKASReference.Equals(model.UKASReference, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        ModelState.AddModelError(nameof(model.UKASReference), "This UKAS reference number already exists");
                    }
                }
                else
                {
                    var userAccount = await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
                    var createdDocument = model.IsFromSummary ?
                        await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, document, submitType == Constants.SubmitType.Save) :
                        await _cabAdminService.CreateDocumentAsync(userAccount, document, submitType == Constants.SubmitType.Save);
                    if (createdDocument == null)
                    {
                        ModelState.AddModelError(string.Empty, "Failed to create the document, please try again.");
                    }
                    else if (submitType == Constants.SubmitType.Continue)
                    {
                        return model.IsFromSummary ?
                            RedirectToAction("Summary", "CAB", new { Area = "admin", id = createdDocument.CABId }) :
                            RedirectToAction("Contact", "CAB", new { Area = "admin", id = createdDocument.CABId });
                    }
                    else
                    {
                        return SaveDraft(document);
                    }
                }
            }

            model.DocumentStatus = document != null ? document.StatusValue : Status.Created;
            return View(model);
        }

        private IActionResult SaveDraft(Document document)
        {
            TempData[Constants.TempDraftKey] = $"Draft record saved for {document.Name} <br>CAB number {document.CABNumber}";
            return RedirectToAction("CABManagement", "Admin", new { Area = "admin" });
        }

        [HttpGet]
        [Route("admin/cab/contact/{id}")]
        public async Task<IActionResult> Contact(string id, bool fromSummary)
        {
            var latest = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latest == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "Admin", new { Area = "admin" });
            }
            // Pre-populate model for edit
            var model = new CABContactViewModel(latest);
            model.IsFromSummary = fromSummary;
            return View(model);
        }

        [HttpPost]
        [Route("admin/cab/contact/{id}")]
        public async Task<IActionResult> Contact(string id, CABContactViewModel model, string submitType, bool fromSummary)
        {
            var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestDocument == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "Admin", new { Area = "admin" });
            }

            if (ModelState.IsValid || submitType == Constants.SubmitType.Save)
            {
                latestDocument.AddressLine1 = model.AddressLine1;
                latestDocument.AddressLine2 = model.AddressLine2;
                latestDocument.TownCity = model.TownCity;
                latestDocument.County = model.County;
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

                var userAccount = await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestDocument, submitType == Constants.SubmitType.Save);
                if (submitType == Constants.SubmitType.Continue)
                {
                    return model.IsFromSummary ?
                        RedirectToAction("Summary", "CAB", new { Area = "admin", id = latestDocument.CABId }) :
                        RedirectToAction("BodyDetails", "CAB", new { Area = "admin", id = latestDocument.CABId });
                }
                return SaveDraft(latestDocument);
            }

            model.DocumentStatus = latestDocument.StatusValue;
            model.IsFromSummary = fromSummary;
            return View(model);
        }


        [HttpGet]
        [Route("admin/cab/body-details/{id}")]
        public async Task<IActionResult> BodyDetails(string id, bool fromSummary)
        {
            var latest = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latest == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "Admin", new { Area = "admin" });
            }

            // Pre-populate model for edit
            var model = new CABBodyDetailsViewModel(latest);

            // Ensure legislative areas are full covered
            if (model.ProductScheduleLegislativeAreas.Except(model.LegislativeAreas).Any())
            {
                var userAccount = await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
                latest.LegislativeAreas = GetLAUnion(model.LegislativeAreas, model.ProductScheduleLegislativeAreas);
                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latest, false);
                model.LegislativeAreas = latest.LegislativeAreas;
            }

            if (!model.TestingLocations.Any())
            {
                model.TestingLocations.Add(string.Empty);
            }

            model.DocumentStatus = latest.StatusValue;
            model.IsFromSummary = fromSummary;
            return View(model);
        }

        private List<string> GetLAUnion(List<string> las, List<string> pschLAs)
        {

            var union = (las ?? new List<string>()).Union(pschLAs).ToList();
            return union;
        }

        [HttpPost]
        [Route("admin/cab/body-details/{id}")]
        public async Task<IActionResult> BodyDetails(string id, CABBodyDetailsViewModel model, string submitType, bool fromSummary)
        {
            var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestDocument == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "Admin", new { Area = "admin" });
            }
            model.LegislativeAreas = GetLAUnion(model.LegislativeAreas, model.ProductScheduleLegislativeAreas ?? new List<string>());
            ModelState.Clear();
            TryValidateModel(model);

            var userAccount = await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
            model.TestingLocations = model.TestingLocations != null ? model.TestingLocations.Where(t => !string.IsNullOrWhiteSpace(t)).ToList() : new List<string>();
            if (submitType == "Add" && model.TestingLocations.Any())
            {
                latestDocument.TestingLocations = model.TestingLocations;
                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestDocument, submitType == Constants.SubmitType.Save);
                model.TestingLocations.Add(string.Empty);
                ModelState.Clear();
            }
            else if (submitType.StartsWith("Remove") && model.TestingLocations.Any())
            {
                var locationToRemove = submitType.Replace("Remove-", string.Empty);
                model.TestingLocations.Remove(locationToRemove);
                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestDocument, submitType == Constants.SubmitType.Save);
                ModelState.Clear();
            }
            else if (ModelState.IsValid || submitType == Constants.SubmitType.Save)
            {
                latestDocument.TestingLocations = model.TestingLocations;
                latestDocument.BodyTypes = model.BodyTypes;
                latestDocument.LegislativeAreas = model.LegislativeAreas;

                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestDocument, submitType == Constants.SubmitType.Save);
                if (submitType == Constants.SubmitType.Continue)
                {
                    return model.IsFromSummary ?
                        RedirectToAction("Summary", "CAB", new { Area = "admin", id = latestDocument.CABId }) :
                        RedirectToAction("SchedulesUpload", "FileUpload", new { Area = "admin", id = latestDocument.CABId });
                }
                return SaveDraft(latestDocument);
            }

            if (!model.TestingLocations.Any())
            {
                model.TestingLocations.Add(string.Empty);
            }

            model.DocumentStatus = latestDocument.StatusValue;
            model.IsFromSummary = fromSummary;
            model.ProductScheduleLegislativeAreas =
                latestDocument.Schedules?.Select(sch => sch.LegislativeArea).Distinct().ToList() ?? new List<string>();
            return View(model);
        }

        [HttpGet]
        [Route("admin/cab/summary/{id}")]
        public async Task<IActionResult> Summary(string id, string? returnUrl)
        {
            var latest = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latest == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "Admin", new { Area = "admin" });
            }

            // Pre-populate model for edit
            var cabDetails = new CABDetailsViewModel(latest);
            var cabContact = new CABContactViewModel(latest);
            var cabBody = new CABBodyDetailsViewModel(latest);
            var model = new CABSummaryViewModel
            {
                CABId = latest.CABId,
                CabDetailsViewModel = cabDetails,
                CabContactViewModel = cabContact,
                CabBodyDetailsViewModel = cabBody,
                Schedules = latest.Schedules ?? new List<FileUpload>(),
                Documents = latest.Documents ?? new List<FileUpload>(),
                ReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? WebUtility.UrlDecode(string.Empty) : WebUtility.UrlDecode(returnUrl),
                CABNameAlreadyExists = await _cabAdminService.DocumentWithSameNameExistsAsync(latest) && latest.StatusValue != Status.Published

            };

            model.ValidCAB = latest.StatusValue != Status.Published
                             && TryValidateModel(cabDetails)
                             && TryValidateModel(cabContact)
                             && TryValidateModel(cabBody);
            ModelState.Clear(); // TODO: clear this to fix error in title but may need to do something else when we highlight publish blocking errors
            return View(model);
        }

        [HttpPost]
        [Route("admin/cab/summary/{id}")]
        public async Task<IActionResult> Summary(CABSummaryViewModel model, string submitType)
        {
            var latest = await _cabAdminService.GetLatestDocumentAsync(model.CABId);
            if (latest == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "Admin", new { Area = "admin" });
            }

            if (submitType == Constants.SubmitType.Save)
            {
                var userAccount = await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latest, submitType == Constants.SubmitType.Save);
                return RedirectToAction("CABManagement", "Admin", new { Area = "admin" });
            }

            // Pre-populate model for edit
            var publishModel = new CABSummaryViewModel
            {
                CABId = latest.CABId,
                CabDetailsViewModel = new CABDetailsViewModel(latest),
                CabContactViewModel = new CABContactViewModel(latest),
                CabBodyDetailsViewModel = new CABBodyDetailsViewModel(latest),
                Schedules = latest.Schedules ?? new List<FileUpload>(),
                Documents = latest.Documents ?? new List<FileUpload>(),
            };
            ModelState.Clear();

            publishModel.ValidCAB = TryValidateModel(publishModel);
            if (publishModel.ValidCAB && submitType == Constants.SubmitType.Continue)
            {
                var userAccount = await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
                var pubishedDoc = await _cabAdminService.PublishDocumentAsync(userAccount, latest);
                TempData["Confirmation"] = true;
                return RedirectToAction("Confirmation", "CAB", new { Area = "admin", id = latest.CABId });
            }

            publishModel.ShowError = true;
            return View(publishModel);
        }

        [HttpGet]
        [Route("admin/cab/confirmation/{id}")]
        public async Task<IActionResult> Confirmation(string id)
        {
            var latest = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latest == null || latest.StatusValue != Status.Published)
            {
                return RedirectToAction("CABManagement", "Admin", new { Area = "admin" });
            }
            return View(new CABConfirmationViewModel
            {
                Name = latest.Name,
                URLSlug = latest.URLSlug,
                CABNumber = latest.CABNumber
            });
        }

        [HttpGet]
        [Route("admin/cab/cancel/{id}")]
        public async Task<IActionResult> Cancel(string id)
        {
            await _cabAdminService.DeleteDraftDocumentAsync(id);
            return RedirectToAction("CABManagement", "Admin", new { Area = "admin" });
        }
    }
}