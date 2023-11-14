using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Notify.Interfaces;
using UKMCAB.Core;
using UKMCAB.Core.Domain.Workflow;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Core.Services.Workflow;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Helpers;
using UKMCAB.Web.UI.Models.ViewModels.Admin;
using UKMCAB.Core.Domain.CAB;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers
{
    [Area("admin"), Authorize]
    public class CABController : Controller
    {
        private readonly ICABAdminService _cabAdminService;
        private readonly IUserService _userService;
        private readonly IWorkflowTaskService _workflowTaskService;
        private readonly IAsyncNotificationClient _notificationClient;
        private readonly CoreEmailTemplateOptions _templateOptions;

        public static class Routes
        {
            public const string CreateNewCab = "cab.create";
            public const string EditCabAbout = "cab.edit.about";
            public const string CabPublishedConfirmation = "cab.published.confirmation";
            public const string CabSubmittedForApprovalConfirmation = "cab.submitted-for-approval.confirmation";
            public const string CabSummary = "cab.summary";
        }

        public CABController(ICABAdminService cabAdminService, IUserService userService, IWorkflowTaskService workflowTaskService, IAsyncNotificationClient notificationClient, IOptions<CoreEmailTemplateOptions> templateOptions)
        {
            _cabAdminService = cabAdminService;
            _userService = userService;
            _workflowTaskService = workflowTaskService;
            _notificationClient = notificationClient;
            _templateOptions = templateOptions.Value;
        }

        [HttpGet("admin/cab/about/{id}", Name = Routes.EditCabAbout)]
        public async Task<IActionResult> About(string id, bool fromSummary)
        {
            var model = (await _cabAdminService.GetLatestDocumentAsync(id)).Map(x => new CABDetailsViewModel(x)) ?? new CABDetailsViewModel { IsNew = true };
            model.IsFromSummary = fromSummary;
            return View(model);
        }

        [HttpPost("admin/cab/about/{id}", Name = Routes.EditCabAbout)]
        public async Task<IActionResult> About(string id, CABDetailsViewModel model, string submitType)
        {
            var appointmentDate = DateUtils.CheckDate(ModelState, model.AppointmentDateDay, model.AppointmentDateMonth, model.AppointmentDateYear, nameof(model.AppointmentDate), "appointment");
            var reviewDate = DateUtils.CheckDate(ModelState, model.ReviewDateDay, model.ReviewDateMonth, model.ReviewDateYear, nameof(model.ReviewDate), "review", appointmentDate);

            var document = await _cabAdminService.GetLatestDocumentAsync(id);
            model.IsNew = document == null;

            //Add 18 months for renew Date
            if (submitType == Constants.SubmitType.Add18)
            {
                var createdAudit = document?.AuditLog?.FirstOrDefault(al => al.Action == AuditCABActions.Created);
                var publishedAudit = document?.AuditLog?.FirstOrDefault(al => al.Action == AuditCABActions.Published);
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
                        AuditLog = new List<Audit>(),
                        CABId = id,
                    };
                }

                if ((string.IsNullOrWhiteSpace(document.URLSlug) || !document.Name.Equals(model.Name)))
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
                document.CabNumberVisibility = model.CabNumberVisibility;
                document.AppointmentDate = appointmentDate;
                document.RenewalDate = reviewDate;
                document.UKASReference = model.UKASReference;

                var duplicateDocuments = await _cabAdminService.FindOtherDocumentsByCabNumberOrUkasReference(document.CABId, document.CABNumber, document.UKASReference);
                if (duplicateDocuments.Any())
                {
                    if (model.CABNumber != null && duplicateDocuments.Any(d => d.CabNumber.DoesEqual(model.CABNumber)))
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
                    var userAccount =
                        await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier))
                            .Value) ?? throw new InvalidOperationException("User account not found");
                    var createdDocument = !model.IsNew
                        ? await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, document)
                        : await _cabAdminService.CreateDocumentAsync(userAccount, document,
                            submitType == Constants.SubmitType.Save);
                   
                    if (submitType == Constants.SubmitType.Continue)
                    {
                        return !model.IsNew
                            ? RedirectToAction("Summary", "CAB", new { Area = "admin", id = createdDocument.CABId })
                            : RedirectToAction("Contact", "CAB", new { Area = "admin", id = createdDocument.CABId });
                    }
                    else
                    {
                        return SaveDraft(document);
                    }
                }
            }

            model.DocumentStatus = document?.StatusValue ?? Status.Draft;
            return View(model);
        }

        [HttpGet("admin/cab/body-details/{id}")]
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
                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latest);
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

        [HttpPost("admin/cab/body-details/{id}")]
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
                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestDocument);
                model.TestingLocations.Add(string.Empty);
                ModelState.Clear();
            }
            else if (submitType.StartsWith("Remove") && model.TestingLocations.Any())
            {
                var locationToRemove = submitType.Replace("Remove-", string.Empty);
                model.TestingLocations.Remove(locationToRemove);
                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestDocument);
                ModelState.Clear();
            }
            else if (ModelState.IsValid || submitType == Constants.SubmitType.Save)
            {
                latestDocument.TestingLocations = model.TestingLocations;
                latestDocument.BodyTypes = model.BodyTypes;
                latestDocument.LegislativeAreas = model.LegislativeAreas;

                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestDocument);
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

        [HttpGet("admin/cab/confirmation/{id}", Name = Routes.CabPublishedConfirmation)]
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

        [HttpGet("admin/cab/submitted-for-approval/{id}", Name = Routes.CabSubmittedForApprovalConfirmation)]
        public async Task<IActionResult> CabSubmittedForApprovalConfirmationAsync(string id)
        {
            var latest = await _cabAdminService.GetLatestDocumentAsync(id);
            return View(new CabSubmittedForApprovalConfirmationViewModel
            {
                Id = id,
                Name = latest.Name,
            });
        }

        [HttpGet("admin/cab/contact/{id}")]
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

        [HttpPost("admin/cab/contact/{id}")]
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
                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestDocument);
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

        [HttpGet("admin/cab/create", Name = Routes.CreateNewCab)]
        public IActionResult Create() => RedirectToRoute(Routes.EditCabAbout, new { id = Guid.NewGuid() });

        [HttpGet("admin/cab/summary/{id}", Name = Routes.CabSummary)]
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
                CABNameAlreadyExists = await _cabAdminService.DocumentWithSameNameExistsAsync(latest) && latest.StatusValue != Status.Published,
                SubStatus = latest.SubStatus,
            };

            model.ValidCAB = latest.StatusValue != Status.Published
                             && TryValidateModel(cabDetails)
                             && TryValidateModel(cabContact)
                             && TryValidateModel(cabBody);
            ModelState.Clear();

            return View(model);
        }

        [HttpPost("admin/cab/summary/{id}", Name = Routes.CabSummary)]
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
                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latest);
                return RedirectToAction("CABManagement", "Admin", new { Area = "admin" });
            }

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
            if (publishModel.ValidCAB)
            {
                var userAccount = await User.GetUserId().MapAsync(x => _userService.GetAsync(x!));
                if (submitType == Constants.SubmitType.Continue) // publish
                {
                    _ = await _cabAdminService.PublishDocumentAsync(userAccount, latest);
                    return RedirectToRoute(Routes.CabPublishedConfirmation, new { id = latest.CABId });
                }

                if (submitType == Constants.SubmitType.SubmitForApproval)
                {
                    await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latest, true);
                    // to-do notification and email
                    var personalisation = new Dictionary<string, dynamic?>
                    {
                        {"UserGroup", Roles.UKAS.Label},
                        {"CABName", latest.Name},
                        {"NotificationsUrl", Url.RouteUrl(NotificationController.Routes.Notifications) ?? throw new InvalidOperationException()},
                        {"CABManagementUrl" , Url.RouteUrl(AdminController.Routes.CABManagement) ?? throw new InvalidOperationException()}
                    };
                    await _notificationClient.SendEmailAsync(_templateOptions.NotificationRequestToPublishEmail, _templateOptions.NotificationRequestToPublish, personalisation);
                    // await _workflowTaskService.CreateAsync(new WorkflowTask(Guid.NewGuid(), TaskType.RequestToPublish))
                    return RedirectToRoute(Routes.CabSubmittedForApprovalConfirmation, new { id = latest.CABId });
                }
            }

            publishModel.ShowError = true;
            return View(publishModel);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if(context.Result is ViewResult viewResult)
            {
                if (viewResult.Model is CABSummaryViewModel summaryViewModel)
                {
                    summaryViewModel.CanPublish = User.IsInRole(Roles.OPSS.Id);
                    summaryViewModel.CanSubmitForApproval = User.IsInRole(Roles.UKAS.Id);
                    summaryViewModel.CanEdit = summaryViewModel.SubStatus != SubStatus.PendingApproval;
                }
                else if (viewResult.Model is CABDetailsViewModel detailsViewModel)
                {
                    detailsViewModel.IsCabNumberDisabled = !User.IsInRole(Roles.OPSS.Id);
                }
            }

            base.OnActionExecuted(context); 
        }

        private List<string> GetLAUnion(List<string> las, List<string> pschLAs)
        {
            var union = (las ?? new List<string>()).Union(pschLAs).ToList();
            return union;
        }

        private IActionResult SaveDraft(Document document)
        {
            TempData[Constants.TempDraftKey] = $"Draft record saved for {document.Name} <br>CAB number {document.CABNumber}";
            return RedirectToAction("CABManagement", "Admin", new { Area = "admin" });
        }
    }
}