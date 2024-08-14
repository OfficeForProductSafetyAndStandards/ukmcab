using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AngleSharp.Common;
using Microsoft.Extensions.Options;
using Notify.Interfaces;
using UKMCAB.Core.Domain.Workflow;
using UKMCAB.Core.EmailTemplateOptions;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Core.Services.Workflow;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.Users;
using UKMCAB.Web.UI.Helpers;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;
using UKMCAB.Web.UI.Models.ViewModels.Shared;
using UKMCAB.Web.UI.Services;
using UKMCAB.Core.Extensions;
using UKMCAB.Web.UI.Models.Builders;
using UKMCAB.Data;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers
{
    [Area("admin"), Authorize]
    public class CABController : UI.Controllers.ControllerBase
    {
        private readonly ICABAdminService _cabAdminService;
        private readonly IWorkflowTaskService _workflowTaskService;
        private readonly IAsyncNotificationClient _notificationClient;
        private readonly CoreEmailTemplateOptions _templateOptions;
        private readonly IEditLockService _editLockService;
        private readonly IUserNoteService _userNoteService;
        private readonly ILegislativeAreaService _legislativeAreaService;
        private readonly ICabSummaryViewModelBuilder _cabSummaryViewModelBuilder;
        private readonly ICabLegislativeAreasViewModelBuilder _cabLegislativeAreasViewModelBuilder;
        private readonly ICabSummaryUiService _cabSummaryUiService;

        public static class Routes
        {
            public const string CreateNewCab = "cab.create";
            public const string EditCabAbout = "cab.edit.about";
            public const string CabPublishedConfirmation = "cab.published.confirmation";
            public const string CabSubmittedForApprovalConfirmation = "cab.submitted-for-approval.confirmation";
            public const string CabSummary = "cab.summary";
            public const string CabPublish = "cab.publish";
            public const string CabGovernmentUserNotes = "cab.governmentusernotes";
            public const string AddLegislativeArea = "cab.addlegislativearea";
            public const string CabHistory = "cab.history";
        }

        public CABController(
            ICABAdminService cabAdminService,
            IUserService userService,
            IWorkflowTaskService workflowTaskService,
            IAsyncNotificationClient notificationClient,
            IOptions<CoreEmailTemplateOptions> templateOptions,
            IEditLockService editLockService,
            IUserNoteService userNoteService,
            ILegislativeAreaService legislativeAreaService,
            ICabSummaryViewModelBuilder cabSummaryViewModelBuilder,
            ICabLegislativeAreasViewModelBuilder cabLegislativeAreasViewModelBuilder,
            ICabSummaryUiService cabSummaryUiService) : base(userService)
        {
            _cabAdminService = cabAdminService;
            _workflowTaskService = workflowTaskService;
            _notificationClient = notificationClient;
            _editLockService = editLockService;
            _templateOptions = templateOptions.Value;
            _userNoteService = userNoteService;
            _legislativeAreaService = legislativeAreaService;
            _cabSummaryViewModelBuilder = cabSummaryViewModelBuilder;
            _cabLegislativeAreasViewModelBuilder = cabLegislativeAreasViewModelBuilder;
            _cabSummaryUiService = cabSummaryUiService;
        }

        [HttpGet("admin/cab/about/{id}", Name = Routes.EditCabAbout)]
        [Authorize(Policy = Policies.EditCabPendingApproval)]
        public async Task<IActionResult> About(string id, bool fromSummary, string? returnUrl = null)
        {
            var model = (await _cabAdminService.GetLatestDocumentAsync(id))
                .Map(x => new CABDetailsViewModel(x, User, false, fromSummary, returnUrl)) ??
                    // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
                    new CABDetailsViewModel(User, fromSummary, returnUrl);
            return View(model);
        }

        [HttpPost("admin/cab/about/{id}", Name = Routes.EditCabAbout)]
        [Authorize(Policy = Policies.EditCabPendingApproval)]
        public async Task<IActionResult> About(string id, CABDetailsViewModel model, string submitType)
        {
            var appointmentDate = DateUtils.CheckDate(ModelState, model.AppointmentDateDay, model.AppointmentDateMonth,
                model.AppointmentDateYear, nameof(model.AppointmentDate), "appointment");
            var reviewDate = DateUtils.CheckDate(ModelState, model.ReviewDateDay, model.ReviewDateMonth,
                model.ReviewDateYear, nameof(model.ReviewDate), "review", appointmentDate);

            var document = await _cabAdminService.GetLatestDocumentAsync(id);
            model.IsNew = document == null;

            //Add 18 months for renew Date
            if (submitType == Constants.SubmitType.Add18)
            {
                var createdAudit = document?.AuditLog?.FirstOrDefault(al => al.Action == AuditCABActions.Created);
                var publishedAudit = document?.AuditLog?.FirstOrDefault(al => al.Action == AuditCABActions.Published);
                var autoRenewDate = document != null && createdAudit != null && publishedAudit == null
                    ? createdAudit.DateTime.AddMonths(18)
                    : DateTime.UtcNow.AddMonths(18);

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
                document.PreviousCABNumbers = model.PreviousCABNumbers;
                document.CabNumberVisibility = model.CabNumberVisibility;
                document.AppointmentDate = appointmentDate;
                document.RenewalDate = reviewDate;
                document.UKASReference = model.UKASReference;

                var duplicateDocuments =
                    await _cabAdminService.FindOtherDocumentsByCabNumberOrUkasReference(document.CABId,
                        document.CABNumber, document.UKASReference);
                if (duplicateDocuments.Any())
                {
                    if (model.CABNumber != null && duplicateDocuments.Any(d => d.CABNumber.DoesEqual(model.CABNumber)))
                    {
                        ModelState.AddModelError(nameof(model.CABNumber), "This CAB number already exists\r\n\r\n");
                    }

                    if (duplicateDocuments.Any(d =>
                            d.UKASReference != null && d.UKASReference.Equals(model.UKASReference,
                                StringComparison.CurrentCultureIgnoreCase)))
                    {
                        ModelState.AddModelError(nameof(model.UKASReference),
                            "This UKAS reference number already exists");
                    }
                }
                else
                {
                    var userAccount =
                        await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier))
                            .Value) ?? throw new InvalidOperationException("User account not found");
                    var createdDocument = !model.IsNew
                        ? await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, document)
                        : await _cabAdminService.CreateDocumentAsync(userAccount, document);

                    if (submitType == Constants.SubmitType.Continue)
                    {
                        return !model.IsNew
                            ? RedirectToAction("Summary", "CAB", new { Area = "admin", id = createdDocument.CABId, revealEditActions = true, returnUrl = model.ReturnUrl })
                            : RedirectToAction("Contact", "CAB", new { Area = "admin", id = createdDocument.CABId, returnUrl = model.ReturnUrl });
                    }

                    return SaveDraft(document);
                }
            }

            model.DocumentStatus = document?.StatusValue ?? Status.Draft;
            model.IsOPSSUser = User != null && User.IsInRole(Roles.OPSS.Id);
            return View(model);
        }

        [HttpGet("admin/cab/body-details/{id}")]
        [Authorize(Policy = Policies.EditCabPendingApproval)]
        public async Task<IActionResult> BodyDetails(string id, bool fromSummary, string? returnUrl)
        {
            var latest = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latest == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }

            // Pre-populate model for edit
            var model = new CABBodyDetailsViewModel(latest);

            if (!model.TestingLocations.Any())
            {
                model.TestingLocations.Add(string.Empty);
            }

            model.DocumentStatus = latest.StatusValue;
            model.IsFromSummary = fromSummary;
            model.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpPost("admin/cab/body-details/{id}")]
        [Authorize(Policy = Policies.EditCabPendingApproval)]
        public async Task<IActionResult> BodyDetails(string id, CABBodyDetailsViewModel model, string submitType,
            bool fromSummary)
        {
            var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestDocument == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }

            var userAccount =
                await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
            model.TestingLocations = model.TestingLocations != null
                ? model.TestingLocations.Where(t => !string.IsNullOrWhiteSpace(t)).ToList()
                : new List<string>();
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

                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestDocument);
                if (submitType == Constants.SubmitType.Continue)
                {
                    if (model.IsFromSummary)
                    {
                        return RedirectToAction("Summary", "CAB", new { Area = "admin", id = latestDocument.CABId, revealEditActions = true, returnUrl = model.ReturnUrl });
                    }
                    else if (!latestDocument.DocumentLegislativeAreas.Any())
                    {
                        return RedirectToAction("AddLegislativeArea", "LegislativeAreaDetails", new { Area = "admin", id = latestDocument.CABId, returnUrl = model.ReturnUrl });
                    }
                    else
                    {
                        return RedirectToAction("ReviewLegislativeAreas", "LegislativeAreaReview", new { Area = "admin", id = latestDocument.CABId, returnUrl = model.ReturnUrl });
                    }
                }

                return SaveDraft(latestDocument);
            }

            if (!model.TestingLocations.Any())
            {
                model.TestingLocations.Add(string.Empty);
            }

            model.DocumentStatus = latestDocument.StatusValue;
            model.IsFromSummary = fromSummary;
            return View(model);
        }

        [HttpGet("admin/cab/confirmation/{id}", Name = Routes.CabPublishedConfirmation)]
        public async Task<IActionResult> Confirmation(string id)
        {
            var latest = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latest == null || latest.StatusValue != Status.Published)
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
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
        [Authorize(Policy = Policies.EditCabPendingApproval)]
        public async Task<IActionResult> Contact(string id, bool fromSummary, string? returnUrl)
        {
            var latest = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latest == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }

            // Pre-populate model for edit
            var model = new CABContactViewModel(latest)
            {
                IsFromSummary = fromSummary,
                ReturnUrl = returnUrl
            };
            return View(model);
        }

        [HttpPost("admin/cab/contact/{id}")]
        [Authorize(Policy = Policies.EditCabPendingApproval)]
        public async Task<IActionResult> Contact(string id, CABContactViewModel model, string submitType,
            bool fromSummary)
        {
            var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestDocument == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }

            if (submitType == Constants.SubmitType.Continue)
            {
                if ((!string.IsNullOrWhiteSpace(model.PointOfContactName) ||
                     !string.IsNullOrWhiteSpace(model.PointOfContactEmail) ||
                     !string.IsNullOrWhiteSpace(model.PointOfContactPhone)) &&
                    !model.IsPointOfContactPublicDisplay.HasValue)
                {
                    ModelState.AddModelError("IsPointOfContactPublicDisplay",
                        "Select who should see the point of contact details");
                }
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

                var userAccount =
                    await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestDocument);
                if (submitType == Constants.SubmitType.Continue)
                {
                    return model.IsFromSummary
                        ? RedirectToAction("Summary", "CAB",
                            new { Area = "admin", id = latestDocument.CABId, revealEditActions = true, returnUrl = model.ReturnUrl })
                        : RedirectToAction("BodyDetails", "CAB", new { Area = "admin", id = latestDocument.CABId, returnUrl = model.ReturnUrl });
                }

                return SaveDraft(latestDocument);
            }

            model.DocumentStatus = latestDocument.StatusValue;
            model.IsFromSummary = fromSummary;
            return View(model);
        }

        [HttpGet("admin/cab/create", Name = Routes.CreateNewCab)]
        public IActionResult Create() => RedirectToRoute(Routes.EditCabAbout, new { id = Guid.NewGuid(), returnUrl = "/service-management" });

        [HttpGet("admin/cab/summary/{id}", Name = Routes.CabSummary)]
        public async Task<IActionResult> Summary(string id, string? returnUrl, bool? revealEditActions, bool? fromCabProfilePage)
        {
            var latest = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latest == null)
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }

            var cabSummary = await PopulateCABSummaryViewModel(latest, revealEditActions, returnUrl, fromCabProfilePage);

            var isCabLockedForUser = await _editLockService.IsCabLockedForUser(latest.CABId, User.GetUserId());

            if (isCabLockedForUser)
            {
                await _cabSummaryUiService.LockCabForUser(cabSummary);
            }

            return View(cabSummary);
        }

        private void ValidateCabSummary(CABDetailsViewModel cabDetails, CABContactViewModel cabContact,
            CABBodyDetailsViewModel cabBody, CABLegislativeAreasViewModel cabLegislativeAreas)
        {
            // Perform validation here (in MVC context) to check data annotation rules AND fluent validation rules.
            // Have to clear the ModelState inbetween calls to TryValidateModel with different objects, otherwise incorrect results can be returned.
            // i.e. an earlier false result can cause all later calls to return false even if the objects are valid.
            cabDetails.IsCompleted = TryValidateModel(cabDetails);
            ModelState.Clear();
            cabContact.IsCompleted = TryValidateModel(cabContact);
            ModelState.Clear();
            cabBody.IsCompleted = TryValidateModel(cabBody);
            ModelState.Clear();
            cabLegislativeAreas.IsCompleted = TryValidateModel(cabLegislativeAreas);
            ModelState.Clear();
        }

        [HttpPost("admin/cab/summary/{id}", Name = Routes.CabSummary)]
        [Authorize(Policy = Policies.EditCabPendingApproval)]
        public async Task<IActionResult> Summary(CABSummaryViewModel model, string submitType)
        {
            var latest =
                await _cabAdminService.GetLatestDocumentAsync(model.CABId ?? throw new InvalidOperationException());
            if (latest == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }

            if (model.SelectedPublishType == null && model.IsOpssAdmin)
            {
                ModelState.Clear();
                model = await PopulateCABSummaryViewModel(latest, model.RevealEditActions, model.ReturnUrl, model.FromCabProfilePage);
                ModelState.AddModelError(nameof(model.SelectedPublishType), "Select an option");

                return View("~/Areas/Admin/views/CAB/Summary.cshtml", model);
            }

            if (model.IsOpssAdmin)
            {
                TempData["PublishType"] = model.SelectedPublishType;
            }

            if (submitType == Constants.SubmitType.Approve)
            {
                return RedirectToRoute(ApproveCABController.Routes.Approve, new { id = latest.CABId, returnUrl = model.ReturnUrl });
            }

            if (submitType == Constants.SubmitType.Save)
            {
                return RedirectToCabManagementWithUnlockCab(latest.CABId);
            }

            var publishModel = new CABSummaryViewModel
            {
                CABId = latest.CABId,
                SelectedPublishType = model.SelectedPublishType,
                CabDetailsViewModel = new CABDetailsViewModel(latest, User),
                CabContactViewModel = new CABContactViewModel(latest),
                CabBodyDetailsViewModel = new CABBodyDetailsViewModel(latest),
                CABProductScheduleDetailsViewModel = new CABProductScheduleDetailsViewModel(latest),
                CABSupportingDocumentDetailsViewModel = new CABSupportingDocumentDetailsViewModel(latest)
            };
            ModelState.Clear();

            var validCAB = TryValidateModel(publishModel) &&
                                    publishModel.CABProductScheduleDetailsViewModel.IsCompleted &&
                                    publishModel.CABSupportingDocumentDetailsViewModel.IsCompleted &&
                                    latest.HasActiveLAs();

            if (validCAB)
            {
                var userAccount = await User.GetUserId().MapAsync(x => _userService.GetAsync(x!));
                if (submitType == Constants.SubmitType.Continue || submitType == Constants.SubmitType.Approve) // publish
                {
                    await _cabAdminService.UpdateOrCreateDraftDocumentAsync(
                        userAccount ?? throw new InvalidOperationException(), latest, false);

                    await _editLockService.RemoveEditLockForCabAsync(latest.CABId);
                    

                    return RedirectToRoute(Routes.CabPublish, new { id = latest.CABId });

                }

                if (submitType == Constants.SubmitType.SubmitForApproval)
                {
                    await _cabAdminService.UpdateOrCreateDraftDocumentAsync(
                        userAccount ?? throw new InvalidOperationException(), latest, true);

                    var legislativeAreaSenderEmailIds =
                        _templateOptions.NotificationLegislativeAreaEmails.ToDictionary();
                    var emailsToSends = new List<ValueTuple<string, int, string>>();

                    foreach (var latestDocumentLegislativeArea in latest.DocumentLegislativeAreas)
                    {
                        if (string.IsNullOrWhiteSpace(latestDocumentLegislativeArea.RoleId))
                            throw new ArgumentNullException(nameof(latestDocumentLegislativeArea.RoleId));

                        if (legislativeAreaSenderEmailIds.Keys.All(a => a != latestDocumentLegislativeArea.RoleId))
                            throw new ArgumentException(
                                $"Legislative area email not found - {latestDocumentLegislativeArea.RoleId}",
                                nameof(latestDocumentLegislativeArea.RoleId));

                        var receiverEmailId = legislativeAreaSenderEmailIds[latestDocumentLegislativeArea.RoleId];
                        switch (latestDocumentLegislativeArea.Status)
                        {
                            case LAStatus.PendingApproval:
                            {
                                await SendInternalNotificationOfLegislativeAreaApprovalAsync(Guid.Parse(latest.CABId),
                                    userAccount, latestDocumentLegislativeArea);
                                if (emailsToSends.All(a => a.Item1 != receiverEmailId))
                                {
                                    emailsToSends.Add(new ValueTuple<string, int, string>(receiverEmailId, 1,
                                        latestDocumentLegislativeArea.LegislativeAreaName));
                                }
                                else
                                {
                                    var laName = emailsToSends.First(x => x.Item1 == receiverEmailId);
                                    emailsToSends.Remove(laName);
                                    emailsToSends.Add(new ValueTuple<string, int, string>(receiverEmailId,
                                        laName.Item2 + 1,
                                        string.Concat(laName.Item3, ", ",
                                            latestDocumentLegislativeArea.LegislativeAreaName)));
                                }

                                break;
                            }
                            case LAStatus.PendingSubmissionToRemove :
                            {
                                await SendNotificationOfLegislativeAreaRequestToRemoveArchiveUnArchiveAsync(
                                    Guid.Parse(latest.CABId),
                                    latest.Name, userAccount, receiverEmailId,
                                    latestDocumentLegislativeArea);
                                latestDocumentLegislativeArea.Status = LAStatus.PendingApprovalToRemove;
                                break;
                            }
                            case  LAStatus.PendingSubmissionToArchiveAndArchiveSchedule or LAStatus.PendingSubmissionToArchiveAndRemoveSchedule:
                            {
                                await SendNotificationOfLegislativeAreaRequestToRemoveArchiveUnArchiveAsync(
                                    Guid.Parse(latest.CABId),
                                    latest.Name, userAccount, receiverEmailId,
                                    latestDocumentLegislativeArea);
                                latestDocumentLegislativeArea.Status = latestDocumentLegislativeArea.Status == LAStatus.PendingSubmissionToArchiveAndArchiveSchedule ?  
                                    LAStatus.PendingApprovalToArchiveAndArchiveSchedule : LAStatus.PendingApprovalToArchiveAndRemoveSchedule;
                                break;
                            } 
                            case LAStatus.PendingSubmissionToUnarchive:
                            {
                                await SendNotificationOfLegislativeAreaRequestToRemoveArchiveUnArchiveAsync(
                                    Guid.Parse(latest.CABId),
                                    latest.Name, userAccount, receiverEmailId,
                                    latestDocumentLegislativeArea);
                                latestDocumentLegislativeArea.Status = LAStatus.PendingApprovalToUnarchive;
                                break;
                            }
                        }
                    }

                    await _cabAdminService.UpdateOrCreateDraftDocumentAsync(
                        userAccount ?? throw new InvalidOperationException(), latest, true);

                    await _editLockService.RemoveEditLockForCabAsync(latest.CABId);
                    
                    if (emailsToSends.Count > 0)
                    {
                        emailsToSends.ForEach(async emailsToSend =>
                        {
                            await SendEmailNotificationOfLegislativeAreaApprovalAsync(Guid.Parse(latest.CABId),
                                latest.Name, userAccount, emailsToSend.Item1,
                                emailsToSend.Item3, emailsToSend.Item2);
                        });
                    }
                    else
                    {
                        await SendNotificationForApproveCab(userAccount,
                            latest.Name ?? throw new InvalidOperationException(), publishModel);
                    }
                    return RedirectToRoute(Routes.CabSubmittedForApprovalConfirmation, new { id = latest.CABId });
                }
            }

            throw new InvalidOperationException("CAB invalid");
        }

        private async Task<CABSummaryViewModel> PopulateCABSummaryViewModel(Document latest, bool? revealEditActions, string? returnUrl, bool? fromCabProfilePage)
        {
            var currentUrl = HttpContext.Request.GetRequestUri().PathAndQuery;
            var userId = User.GetUserId();

            await _cabSummaryUiService.CreateDocumentAsync(latest, revealEditActions);

            var legislativeAreas = await _legislativeAreaService.GetLegislativeAreasForDocumentAsync(latest);
            var purposeOfAppointments = await _legislativeAreaService.GetPurposeOfAppointmentsForDocumentAsync(latest);
            var categories = await _legislativeAreaService.GetCategoriesForDocumentAsync(latest);
            var subCategories = await _legislativeAreaService.GetSubCategoriesForDocumentAsync(latest);
            var products = await _legislativeAreaService.GetProductsForDocumentAsync(latest);
            var procedures = await _legislativeAreaService.GetProceduresForDocumentAsync(latest);
            var designatedStandards = await _legislativeAreaService.GetDesignatedStandardsForDocumentAsync(latest);

            var cabNameAlreadyExists = await _cabAdminService.DocumentWithSameNameExistsAsync(latest);
            var isCabLockedForUser = await _editLockService.IsCabLockedForUser(latest.CABId, userId);
            var successBannerMessage = _cabSummaryUiService.GetSuccessBannerMessage();

            var cabDetails = new CABDetailsViewModel(latest, User, cabNameAlreadyExists);
            var cabContact = new CABContactViewModel(latest);
            var cabBody = new CABBodyDetailsViewModel(latest);
            var cabProductSchedules = new CABProductScheduleDetailsViewModel(latest);
            var cabSupportingDocuments = new CABSupportingDocumentDetailsViewModel(latest);
            var cabHistory = new CABHistoryViewModel(latest, currentUrl);
            var cabGovernmentUserNoteViewModel = new CABGovernmentUserNotesViewModel(latest, currentUrl);
            var cabPublishTypeViewModel = new CABPublishTypeViewModel(User != null && User.IsInRole(Roles.OPSS.Id));
            var cabLegislativeAreas = _cabLegislativeAreasViewModelBuilder
                .WithDocumentLegislativeAreas(
                    latest.DocumentLegislativeAreas,
                    legislativeAreas,
                    latest.ScopeOfAppointments,
                    purposeOfAppointments,
                    categories,
                    subCategories,
                    products,
                    procedures,
                    designatedStandards)
                .Build();

            ValidateCabSummary(cabDetails, cabContact, cabBody, cabLegislativeAreas);

            var cabSummary = _cabSummaryViewModelBuilder
                .WithRoleInfo(latest)
                .WithDocumentDetails(latest)
                .WithLegislativeAreasPendingApprovalCount(latest)
                .WithCabDetails(cabDetails)
                .WithCabContactViewModel(cabContact)
                .WithCabBodyDetailsViewModel(cabBody)
                .WithCabLegislativeAreasViewModel(cabLegislativeAreas)
                .WithProductScheduleDetailsViewModel(cabProductSchedules)
                .WithCabSupportingDocumentDetailsViewModel(cabSupportingDocuments)
                .WithCabGovernmentUserNotesViewModel(cabGovernmentUserNoteViewModel)
                .WithCabHistoryViewModel(cabHistory)
                .WithCabPublishTypeViewModel(cabPublishTypeViewModel)
                .WithReturnUrl(returnUrl)
                .WithRevealEditActions(revealEditActions)
                .WithRequestedFromCabProfilePage(fromCabProfilePage)
                .WithIsEditLocked(isCabLockedForUser)
                .WithSuccessBannerMessage(successBannerMessage)
                .Build();

            return cabSummary;
        }

        [HttpGet("admin/cab/publish/{id}", Name = Routes.CabPublish)]
        public async Task<IActionResult> Publish(string id, string? returnUrl)
        {
            var latest = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latest == null || latest.StatusValue != Status.Draft) // Implies no document or document in draft mode
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }

            return View(new PublishCABViewModel
            {
                CABId = id,
                CABName = latest.Name,
                ReturnURL = returnUrl
            });
        }

        [HttpPost("admin/cab/publish/{id}", Name = Routes.CabPublish)]
        public async Task<IActionResult> Publish(PublishCABViewModel model)
        {
            var latest =
                await _cabAdminService.GetLatestDocumentAsync(model.CABId ?? throw new InvalidOperationException());
            if (latest == null || latest.StatusValue != Status.Draft) // Implies no document or document in draft mode
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }

            if (ModelState.IsValid)
            {
                var userAccount = await User.GetUserId().MapAsync(x => _userService.GetAsync(x!));

                var publishType = TempData["PublishType"] != null ? (string)TempData["PublishType"] : DataConstants.PublishType.MajorPublish;

                _ = await _cabAdminService.PublishDocumentAsync(
                    userAccount ?? throw new InvalidOperationException(), latest, model.PublishInternalReason,
                    model.PublishPublicReason, publishType);
                
                return RedirectToRoute(Routes.CabPublishedConfirmation, new { id = latest.CABId });
            }

            model.CABName = latest.Name;            

            return View(model);
        }

        [Authorize(Policy = Policies.GovernmentUserNotes)]
        [HttpGet("admin/cab/governmentusernotes/{cabId}/{cabDocumentId}", Name = Routes.CabGovernmentUserNotes)]
        public async Task<IActionResult> GovernmentUserNotes(Guid cabId, Guid cabDocumentId, string? returnUrl,
            int pagenumber = 1)
        {
            var userNotes = await _userNoteService.GetAllUserNotesForCabDocumentId(cabDocumentId);
            var model = new GovernmentUserNotesViewModel
            {
                CABId = cabId,
                GovernmentUserNotes = new UserNoteListViewModel(cabDocumentId, userNotes, pagenumber),
                ReturnUrl = returnUrl,
            };

            return View(model);
        }

        [HttpGet("admin/cab/history/{cabId}", Name = Routes.CabHistory)]
        public async Task<IActionResult> History(string cabId, string? returnUrl, int pageNumber = 1)
        {
            var latest = await _cabAdminService.GetLatestDocumentAsync(cabId);
            if (latest == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }

            var model = new CABAuditLogHistoryViewModel
            {
                AuditLogHistory = new AuditLogHistoryViewModel(latest.AuditLog, pageNumber),
                ReturnUrl = returnUrl,
            };

            return View("History", model);
        }

        private async Task SendEmailNotificationOfLegislativeAreaApprovalAsync(Guid cabId, string cabName,
            UserAccount userAccount, string legislativeAreaReceiverEmailId, string legislativeAreaName,
            int legislativeAreaCount)
        {
            var user = new User(userAccount.Id, userAccount.FirstName, userAccount.Surname,
                userAccount.Role ?? throw new InvalidOperationException(),
                userAccount.EmailAddress ?? throw new InvalidOperationException());

            var emailBody =
                $"{user.FirstAndLastName} from {user.UserGroup} has requested that the {legislativeAreaName} legislative area is approved for CAB [{cabName}]({UriHelper.GetAbsoluteUriFromRequestAndPath(HttpContext.Request, Url.RouteUrl(Routes.CabSummary, new { id = cabId }))}).";
            if (legislativeAreaCount > 1)
                emailBody =
                    $"{user.FirstAndLastName} from {user.UserGroup} has requested that the following legislative areas are approved for CAB [{cabName}]({UriHelper.GetAbsoluteUriFromRequestAndPath(HttpContext.Request, Url.RouteUrl(Routes.CabSummary, new { id = cabId }))}) : {legislativeAreaName}.";

            var personalisation = new Dictionary<string, dynamic?>
            {
                { "CABName", cabName },                
                { "emailBody", emailBody },
                { "userGroup", user.UserGroup }                
            };
            await _notificationClient.SendEmailAsync(legislativeAreaReceiverEmailId,
                _templateOptions.NotificationLegislativeAreaRequestToPublish, personalisation);
        }

        private async Task SendInternalNotificationOfLegislativeAreaApprovalAsync(Guid cabId, UserAccount userAccount,
           DocumentLegislativeArea documentLegislativeArea)
        {
            var user = new User(userAccount.Id, userAccount.FirstName, userAccount.Surname,
                userAccount.Role ?? throw new InvalidOperationException(),
                userAccount.EmailAddress ?? throw new InvalidOperationException());

            await _workflowTaskService.CreateAsync(
                new WorkflowTask(
                    TaskType.LegislativeAreaApproveRequestForCab,
                    user,
                    documentLegislativeArea.RoleId,
                    null,
                    DateTime.Now,
                    $"{user.FirstAndLastName} from {user.UserGroup} has requested that the {documentLegislativeArea.LegislativeAreaName} legislative area is approved.",
                    user,
                    DateTime.Now,
                    null,
                    null,
                    false,
                    cabId,
                    documentLegislativeArea.Id
                    ));
        }

        private async Task SendNotificationOfLegislativeAreaRequestToRemoveArchiveUnArchiveAsync(Guid cabId, string cabName,
          UserAccount userAccount, string legislativeAreaReceiverEmailId, DocumentLegislativeArea documentLegislativeArea)
        {
            var user = new User(userAccount.Id, userAccount.FirstName, userAccount.Surname,
                userAccount.Role ?? throw new InvalidOperationException(),
                userAccount.EmailAddress ?? throw new InvalidOperationException());
            TaskType? taskType = null;
            
            string? actionText;
            switch (documentLegislativeArea.Status)
            {
                case LAStatus.PendingSubmissionToRemove:
                    actionText = "remove";
                    taskType = TaskType.LegislativeAreaRequestToRemove;
                    break;
                case LAStatus.PendingSubmissionToUnarchive:
                    actionText = "unarchive";
                    taskType = TaskType.LegislativeAreaRequestToUnarchive;
                    break;
                default:
                    actionText = "archive";
                    taskType = TaskType.LegislativeAreaRequestToArchiveAndArchiveSchedule;
                    break;
            }

            var personalisation = new Dictionary<string, dynamic?>
            {
                { "CABName", cabName },
                { "CABUrl",
                    UriHelper.GetAbsoluteUriFromRequestAndPath(HttpContext.Request,
                        Url.RouteUrl(Routes.CabSummary, new { id = cabId }))
                },
                { "userGroup", user.UserGroup },
                { "userName", user.FirstAndLastName },
                { "legislativeAreaName", documentLegislativeArea.LegislativeAreaName },
                { "Reason", documentLegislativeArea.RequestReason },
                { "action", actionText }
            };

            await _notificationClient.SendEmailAsync(legislativeAreaReceiverEmailId,
                _templateOptions.NotificationLegislativeAreaRequestToRemoveArchiveUnArchive, personalisation);

            actionText = string.Concat(actionText, "d");

            await _workflowTaskService.CreateAsync(
                new WorkflowTask(
                    taskType.Value,
                    user,
                    documentLegislativeArea.RoleId,
                    null,
                    DateTime.Now,
                    $"{user.FirstAndLastName} from {user.UserGroup} has requested that the {documentLegislativeArea.LegislativeAreaName} legislative area is {actionText} for {cabName}.",
                    user,
                    DateTime.Now,
                    null,
                    documentLegislativeArea.RequestReason,
                    false,
                    cabId,
                    documentLegislativeArea.Id
                    ));
        }

        /// <summary>
        /// Sends an email and notification for Request to publish a cab
        /// </summary>
        /// <param name="userAccount">User creating the cab</param>
        /// <param name="cabName">Name of CAB</param>
        /// <param name="publishModel">ViewModel to build notification</param>
        private async Task SendNotificationForApproveCab(UserAccount userAccount, string cabName,
            CABSummaryViewModel publishModel)
        {
            var personalisation = new Dictionary<string, dynamic?>
            {
                { "UserGroup", Roles.UKAS.Label },
                { "CABName", cabName },
                {
                    "NotificationsUrl",
                    UriHelper.GetAbsoluteUriFromRequestAndPath(HttpContext.Request,
                        Url.RouteUrl(NotificationController.Routes.Notifications))
                },
                {
                    "CABManagementUrl",
                    UriHelper.GetAbsoluteUriFromRequestAndPath(HttpContext.Request,
                        Url.RouteUrl(CabManagementController.Routes.CABManagement))
                }
            };
            var userRoleId = Roles.List.First(r =>
                r.Label != null && r.Label.Equals(userAccount.Role, StringComparison.CurrentCultureIgnoreCase)).Id;
            await _notificationClient.SendEmailAsync(_templateOptions.ApprovedBodiesEmail,
                _templateOptions.NotificationRequestToPublish, personalisation);
            if (publishModel.CabDetailsViewModel != null)
            {
                await _workflowTaskService.CreateAsync(new WorkflowTask(TaskType.RequestToPublish,
                    new User(userAccount.Id, userAccount.FirstName, userAccount.Surname, userRoleId,
                        userAccount.EmailAddress ?? throw new InvalidOperationException()),
                    Roles.OPSS.Id, null, null,
                    $"{userAccount.FirstName} {userAccount.Surname} from {Roles.NameFor(userRoleId)} has submitted a request to approve and publish {publishModel.CabDetailsViewModel.Name}.",
                    new User(userAccount.Id, userAccount.FirstName, userAccount.Surname, userRoleId,
                        userAccount.EmailAddress ?? throw new InvalidOperationException()), DateTime.Now,
                    null, null,
                    false, Guid.Parse(publishModel.CABId ?? throw new InvalidOperationException())));
            }
        }

        private IActionResult SaveDraft(Document document)
        {
            TempData[Constants.TempDraftKeyLine1] =
                $"Draft record saved for {document.Name}";
            TempData[Constants.TempDraftKeyLine2] = $"CAB number {document.CABNumber}";
            return RedirectToCabManagementWithUnlockCab(document.CABId);
        }

        private RedirectToActionResult RedirectToCabManagementWithUnlockCab(string cabId)
        {
            return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin", unlockCab = cabId });
        }
    }
}