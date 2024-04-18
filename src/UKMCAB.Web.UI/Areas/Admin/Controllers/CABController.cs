using Microsoft.AspNetCore.Authorization;
using System.Net;
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
using UKMCAB.Common.Extensions;
using UKMCAB.Web.UI.Models.ViewModels.Shared;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;
using UKMCAB.Web.UI.Services;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Enums;
using UKMCAB.Common.Extensions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

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
        private readonly ILegislativeAreaDetailService _legislativeAreaDetailService;

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
            ILegislativeAreaDetailService legislativeAreaDetailService) : base(userService)
        {
            _cabAdminService = cabAdminService;
            _workflowTaskService = workflowTaskService;
            _notificationClient = notificationClient;
            _editLockService = editLockService;
            _templateOptions = templateOptions.Value;
            _userNoteService = userNoteService;
            _legislativeAreaService = legislativeAreaService;
            _legislativeAreaDetailService = legislativeAreaDetailService;
        }

        [HttpGet("admin/cab/about/{id}", Name = Routes.EditCabAbout)]
        public async Task<IActionResult> About(string id, bool fromSummary, string returnUrl)
        {
            var model = (await _cabAdminService.GetLatestDocumentAsync(id)).Map(x => new CABDetailsViewModel(x)) ??
                        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
                        new CABDetailsViewModel { IsNew = true };
            model.IsFromSummary = fromSummary;
            model.ReturnUrl = returnUrl;
            model.IsOPSSUser = User.IsInRole(Roles.OPSS.Id);
            model.IsCabNumberDisabled = !User.IsInRole(Roles.OPSS.Id);
            return View(model);
        }

        [HttpPost("admin/cab/about/{id}", Name = Routes.EditCabAbout)]
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
                            ? RedirectToAction("Summary", "CAB", new { Area = "admin", id = createdDocument.CABId, subSectionEditAllowed = true, returnUrl = model.ReturnUrl })
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
                        return RedirectToAction("Summary", "CAB", new { Area = "admin", id = latestDocument.CABId, subSectionEditAllowed = true, returnUrl = model.ReturnUrl });
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
                            new { Area = "admin", id = latestDocument.CABId, subSectionEditAllowed = true, returnUrl = model.ReturnUrl })
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
        public async Task<IActionResult> Summary(string id, string? returnUrl, bool? subSectionEditAllowed)
        {
            var latest = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latest == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }

            if (latest.StatusValue == Status.Published && subSectionEditAllowed == true)
            {
                var userAccount =
                    await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
                await _cabAdminService.CreateDocumentAsync(userAccount!, latest);
            }

            //Check Edit lock
            var userIdWithLock = await _editLockService.LockExistsForCabAsync(latest.CABId);
            var isEditLocked = !string.IsNullOrWhiteSpace(userIdWithLock) && User.GetUserId() != userIdWithLock;
            var userInCreatorUserGroup = User.IsInRole(latest.CreatedByUserGroup);
            var laPendingApprovalCount = LAPendingApprovalCountForUser(latest, UserRoleId == Roles.OPSS.Id);
            var isOgdUser = Roles.OgdRolesList.Contains(UserRoleId);
            var showOgdActions = isOgdUser && subSectionEditAllowed.HasValue && subSectionEditAllowed.Value && !isEditLocked && 
                latest.IsPendingOgdApproval && laPendingApprovalCount > 0;
            if (showOgdActions)
            {
                await _cabAdminService.FilterCabContentsByLaIfPendingOgdApproval(latest, UserRoleId);
            }

            // Pre-populate model for edit
            var cabDetails = new CABDetailsViewModel(latest)
            {
                IsCabNumberDisabled = !User.IsInRole(Roles.OPSS.Id)
            };
            var cabContact = new CABContactViewModel(latest);
            var cabBody = new CABBodyDetailsViewModel(latest);
            var cabLegislativeAreas = await PopulateCABLegislativeAreasViewModelAsync(latest);
            ValidateCabSummaryModels(cabDetails, cabContact, cabBody, cabLegislativeAreas);

            var cabProductSchedules = new CABProductScheduleDetailsViewModel(latest);
            var cabSupportingDocuments = new CABSupportingDocumentDetailsViewModel(latest);

            var auditLogOrdered = latest.AuditLog.OrderBy(a => a.DateTime).ToList();
            var publishedAudit = auditLogOrdered.LastOrDefault(al => al.Action == AuditCABActions.Published);
            var model = new CABSummaryViewModel
            {
                Id = latest.id,
                CABId = latest.CABId,
                CabDetailsViewModel = cabDetails,
                CabContactViewModel = cabContact,
                CabBodyDetailsViewModel = cabBody,
                CabLegislativeAreasViewModel = cabLegislativeAreas,
                CABProductScheduleDetailsViewModel = cabProductSchedules,
                CABSupportingDocumentDetailsViewModel = cabSupportingDocuments,
                ReturnUrl = string.IsNullOrWhiteSpace(returnUrl)
                    ? WebUtility.UrlDecode(string.Empty)
                    : WebUtility.UrlDecode(returnUrl),
                CABNameAlreadyExists = await _cabAdminService.DocumentWithSameNameExistsAsync(latest) &&
                                       latest.StatusValue != Status.Published,
                Status = latest.StatusValue,
                StatusCssStyle = CssClassUtils.CabStatusStyle(latest.StatusValue),
                SubStatus = latest.SubStatus,
                SubStatusName = latest.SubStatus.GetEnumDescription(),
                ValidCAB = latest.StatusValue != Status.Published
                           && cabDetails.IsCompleted && cabContact.IsCompleted && cabBody.IsCompleted &&
                           cabLegislativeAreas.IsCompleted
                           && cabProductSchedules.IsCompleted && cabSupportingDocuments.IsCompleted,
                TitleHint = "CAB profile",
                Title = User.IsInRole(Roles.OPSS.Id) ? latest.SubStatus == SubStatus.PendingApprovalToPublish
                        ? "Check details before approving or declining"
                        : "Check details before publishing"
                    : userInCreatorUserGroup ? "Check details before submitting for approval" : "Summary",
                IsOPSSOrInCreatorUserGroup = User.IsInRole(Roles.OPSS.Id) || userInCreatorUserGroup,
                IsEditLocked = isEditLocked,
                SubSectionEditAllowed = subSectionEditAllowed ?? false,
                LastModifiedDate = latest.LastUpdatedDate,
                PublishedDate = publishedAudit?.DateTime ?? null,
                GovernmentUserNoteCount = latest.GovernmentUserNotes?.Count ?? 0,
                LastGovernmentUserNoteDate = Enumerable.MaxBy(latest.GovernmentUserNotes!, u => u.DateTime)?.DateTime,
                LastAuditLogHistoryDate = Enumerable.MaxBy(latest.AuditLog!, u => u.DateTime)?.DateTime,
                IsPendingOgdApproval = latest.IsPendingOgdApproval,
                IsMatchingOgdUser = laPendingApprovalCount > 0,
                ShowOgdActions = showOgdActions,
                LegislativeAreasPendingApprovalCount = laPendingApprovalCount,
                IsOpssAdmin = UserRoleId == Roles.OPSS.Id,
                LegislativeAreasApprovedByAdminCount = latest.DocumentLegislativeAreas.Count(dla => dla.Status is LAStatus.ApprovedByOpssAdmin or
                LAStatus.ApprovedToRemoveByOpssAdmin or LAStatus.ApprovedToArchiveAndArchiveScheduleByOpssAdmin or LAStatus.ApprovedToArchiveAndRemoveScheduleByOpssAdmin
                ),
                LegislativeAreaHasBeenActioned = latest.DocumentLegislativeAreas.Any(la => la.Status is LAStatus.Approved or LAStatus.Declined or LAStatus.ApprovedByOpssAdmin or LAStatus.DeclinedByOpssAdmin or LAStatus.ApprovedToRemoveByOpssAdmin or LAStatus.ApprovedToArchiveAndArchiveScheduleByOpssAdmin or LAStatus.ApprovedToArchiveAndRemoveScheduleByOpssAdmin)
            };

            //Lock Record for edit
            if (string.IsNullOrWhiteSpace(userIdWithLock) && model.SubSectionEditAllowed
                                                          && latest.StatusValue is Status.Draft or Status.Published
                                                          && model.IsOPSSOrInCreatorUserGroup)
            {
                await _editLockService.SetAsync(latest.CABId, User.GetUserId()!);
            }

            var draftUpdated = Enumerable.MaxBy(
                latest.AuditLog.Where(l => l.Action == AuditCABActions.Created),
                u => u.DateTime)?.DateTime != latest.LastUpdatedDate;
            model.CanPublish = User.IsInRole(Roles.OPSS.Id) && draftUpdated;
            model.CanSubmitForApproval = User.IsInRole(Roles.UKAS.Id) && draftUpdated;
            model.ShowEditActions = model is { SubSectionEditAllowed: true, IsEditLocked: false } &&
                                    ((model.SubStatus != SubStatus.PendingApprovalToPublish && model.IsOPSSOrInCreatorUserGroup) ||
                                     (model.SubStatus == SubStatus.PendingApprovalToPublish && model.IsOpssAdmin && model.LegislativeAreaHasBeenActioned));
            model.EditByGroupPermitted =
                model.SubStatus != SubStatus.PendingApprovalToPublish &&
                (model.Status == Status.Published || model.IsOPSSOrInCreatorUserGroup);

            if (TempData.ContainsKey(Constants.ApprovedLA))
            {
                TempData.Remove(Constants.ApprovedLA);
                model.SuccessBannerMessage = "Legislative area has been approved.";
            }
            if (TempData.ContainsKey(Constants.DeclinedLA))
            {
                TempData.Remove(Constants.DeclinedLA);
                model.SuccessBannerMessage = "Legislative area has been declined.";
            }
            return View(model);
        }

        [HttpPost("admin/cab/summary/{id}", Name = Routes.CabSummary)]
        public async Task<IActionResult> Summary(CABSummaryViewModel model, string submitType)
        {
            var latest =
                await _cabAdminService.GetLatestDocumentAsync(model.CABId ?? throw new InvalidOperationException());
            if (latest == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }

            if (submitType == Constants.SubmitType.Save)
            {
                return RedirectToCabManagementWithUnlockCab(latest.CABId);
            }

            var publishModel = new CABSummaryViewModel
            {
                CABId = latest.CABId,
                CabDetailsViewModel = new CABDetailsViewModel(latest),
                CabContactViewModel = new CABContactViewModel(latest),
                CabBodyDetailsViewModel = new CABBodyDetailsViewModel(latest),
                CABProductScheduleDetailsViewModel = new CABProductScheduleDetailsViewModel(latest),
                CABSupportingDocumentDetailsViewModel = new CABSupportingDocumentDetailsViewModel(latest)
            };
            ModelState.Clear();

            publishModel.ValidCAB = TryValidateModel(publishModel) &&
                                    publishModel.CABProductScheduleDetailsViewModel.IsCompleted &&
                                    publishModel.CABSupportingDocumentDetailsViewModel.IsCompleted;

            if (publishModel.ValidCAB)
            {
                var userAccount = await User.GetUserId().MapAsync(x => _userService.GetAsync(x!));
                if (submitType == Constants.SubmitType.Continue) // publish
                {
                    await _editLockService.RemoveEditLockForCabAsync(latest.CABId);
                    return RedirectToRoute(Routes.CabPublish, new { id = latest.CABId });
                }

                if (submitType == Constants.SubmitType.SubmitForApproval)
                {
                    await _cabAdminService.UpdateOrCreateDraftDocumentAsync(
                        userAccount ?? throw new InvalidOperationException(), latest, true);
                    await SendNotificationForApproveCab(userAccount,
                        latest.Name ?? throw new InvalidOperationException(), publishModel);

                    await _editLockService.RemoveEditLockForCabAsync(latest.CABId);
                    var legislativeAreaSenderEmailIds =
                        _templateOptions.NotificationLegislativeAreaEmails.ToDictionary();
                    var emailsToSends = new List<ValueTuple<string, int, string>>();

                    var updateCab = false;

                    foreach (var latestDocumentLegislativeArea in latest.DocumentLegislativeAreas)
                    {
                        if (string.IsNullOrWhiteSpace(latestDocumentLegislativeArea.RoleId))
                            throw new ArgumentNullException(nameof(latestDocumentLegislativeArea.RoleId));

                        if (legislativeAreaSenderEmailIds.Keys.All(a => a != latestDocumentLegislativeArea.RoleId))
                            throw new ArgumentException(
                                $"Legislative area email not found - {latestDocumentLegislativeArea.RoleId}",
                                nameof(latestDocumentLegislativeArea.RoleId));

                        var receiverEmailId = legislativeAreaSenderEmailIds[latestDocumentLegislativeArea.RoleId];
                        if (latestDocumentLegislativeArea.Status == LAStatus.PendingApproval)
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
                        }
                        else if(latestDocumentLegislativeArea.Status == LAStatus.PendingSubmissionToRemove ||
                            latestDocumentLegislativeArea.Status == LAStatus.PendingSubmissionToArchiveAndArchiveSchedule ||
                            latestDocumentLegislativeArea.Status == LAStatus.PendingSubmissionToArchiveAndRemoveSchedule)
                        {
                            await SendNotificationOfLegislativeAreaRequestToRemoveArchiveUnArchiveAsync(Guid.Parse(latest.CABId),
                                latest.Name, userAccount, receiverEmailId,
                                latestDocumentLegislativeArea);

                            latestDocumentLegislativeArea.Status = LAStatus.PendingApprovalToRemove;

                            if(!updateCab)
                            {
                                updateCab = true;
                            }
                        }

                        if(updateCab)
                        {
                            await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latest);
                        }
                    }

                    emailsToSends.ForEach(async emailsToSend =>
                    {
                        await SendEmailNotificationOfLegislativeAreaApprovalAsync(Guid.Parse(latest.CABId),
                            latest.Name, userAccount, emailsToSend.Item1,
                            emailsToSend.Item3, emailsToSend.Item2);
                    });

                    return RedirectToRoute(Routes.CabSubmittedForApprovalConfirmation, new { id = latest.CABId });
                }
            }

            throw new InvalidOperationException("CAB invalid");
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

                _ = await _cabAdminService.PublishDocumentAsync(
                    userAccount ?? throw new InvalidOperationException(), latest, model.PublishInternalReason,
                    model.PublishPublicReason);

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
            var userRoleId = Roles.List.First(r => r.Id == userAccount.Role).Id;
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
                { "userGroup", user.UserGroup },
                { "userName", user.FirstAndLastName },
                { "legislativeAreaName", legislativeAreaName }               
            };
            await _notificationClient.SendEmailAsync(legislativeAreaReceiverEmailId,
                _templateOptions.NotificationLegislativeAreaPublishApproved, personalisation);
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

            var actionText = documentLegislativeArea.Status switch
            {
                LAStatus.PendingSubmissionToRemove => "remove",               
                _ => "archive",
            };           

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
                { "Reason", documentLegislativeArea.ReasonToRemoveOrArchive },
                { "action", actionText }
            };

            await _notificationClient.SendEmailAsync(legislativeAreaReceiverEmailId,
                _templateOptions.NotificationLegislativeAreaRequestToRemoveArchiveUnArchive, personalisation);

            actionText = string.Concat(actionText, "d");

            await _workflowTaskService.CreateAsync(
                new WorkflowTask(
                    TaskType.LegislativeAreaRequestToRemove,
                    user,
                    documentLegislativeArea.RoleId,
                    null,
                    DateTime.Now,
                    $"{user.FirstAndLastName} from {user.UserGroup} has requested that the {documentLegislativeArea.LegislativeAreaName} legislative area is {actionText} for {cabName}.",
                    user,
                    DateTime.Now,
                    null,
                    documentLegislativeArea.ReasonToRemoveOrArchive,
                    false,
                    cabId));
        }

        private IActionResult SaveDraft(Document document)
        {
            TempData[Constants.TempDraftKey] =
                $"Draft record saved for {document.Name} <br>CAB number {document.CABNumber}";
            return RedirectToCabManagementWithUnlockCab(document.CABId);
        }

        private RedirectToActionResult RedirectToCabManagementWithUnlockCab(string cabId)
        {
            return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin", unlockCab = cabId });
        }

        private async Task<CABLegislativeAreasViewModel> PopulateCABLegislativeAreasViewModelAsync(Document cab)
        {
            var viewModel = new CABLegislativeAreasViewModel();

            foreach (var documentLegislativeArea in cab.DocumentLegislativeAreas)
            {
                var legislativeArea =
                    await _legislativeAreaService.GetLegislativeAreaByIdAsync(documentLegislativeArea
                        .LegislativeAreaId);

                var legislativeAreaViewModel = new CABLegislativeAreasItemViewModel
                {
                    Name = legislativeArea.Name,
                    IsProvisional = documentLegislativeArea.IsProvisional,
                    IsArchived = documentLegislativeArea.Archived,
                    AppointmentDate = documentLegislativeArea.AppointmentDate,
                    ReviewDate = documentLegislativeArea.ReviewDate,
                    Reason = documentLegislativeArea.Reason,
                    PointOfContactName = documentLegislativeArea.PointOfContactName,
                    PointOfContactEmail = documentLegislativeArea.PointOfContactEmail,
                    PointOfContactPhone = documentLegislativeArea.PointOfContactPhone,
                    IsPointOfContactPublicDisplay = documentLegislativeArea.IsPointOfContactPublicDisplay,
                    CanChooseScopeOfAppointment = legislativeArea.HasDataModel,
                    Status = documentLegislativeArea.Status,
                    StatusCssStyle = CssClassUtils.LAStatusStyle(documentLegislativeArea.Status),
                    RoleName = Roles.NameFor(documentLegislativeArea.RoleId),
                    RoleId = documentLegislativeArea.RoleId,
                };

                var scopeOfAppointments = cab.ScopeOfAppointments.Where(x => x.LegislativeAreaId == legislativeArea.Id);
                foreach (var scopeOfAppointment in scopeOfAppointments)
                {
                    var purposeOfAppointment = scopeOfAppointment.PurposeOfAppointmentId.HasValue
                        ? (await _legislativeAreaService.GetPurposeOfAppointmentByIdAsync(scopeOfAppointment
                            .PurposeOfAppointmentId.Value))?.Name
                        : null;

                    var category = scopeOfAppointment.CategoryId.HasValue
                        ? (await _legislativeAreaService.GetCategoryByIdAsync(scopeOfAppointment.CategoryId.Value))
                        ?.Name
                        : null;

                    var subCategory = scopeOfAppointment.SubCategoryId.HasValue
                        ? (await _legislativeAreaService.GetSubCategoryByIdAsync(scopeOfAppointment.SubCategoryId
                            .Value))?.Name
                        : null;

                    foreach (var productProcedure in scopeOfAppointment.ProductIdAndProcedureIds)
                    {
                        var soaViewModel = new LegislativeAreaListItemViewModel()
                        {
                            LegislativeArea = new ListItem { Id = legislativeArea.Id, Title = legislativeArea.Name },
                            PurposeOfAppointment = purposeOfAppointment,
                            Category = category,
                            SubCategory = subCategory,
                            ScopeId = scopeOfAppointment.Id,
                        };

                        if (productProcedure.ProductId.HasValue)
                        {
                            var product =
                                await _legislativeAreaService.GetProductByIdAsync(productProcedure.ProductId.Value);
                            soaViewModel.Product = product!.Name;
                        }

                        foreach (var procedureId in productProcedure.ProcedureIds)
                        {
                            var procedure = await _legislativeAreaService.GetProcedureByIdAsync(procedureId);
                            soaViewModel.Procedures?.Add(procedure!.Name);
                        }

                        legislativeAreaViewModel.ScopeOfAppointments.Add(soaViewModel);
                    }
                }

                var distinctSoa = legislativeAreaViewModel.ScopeOfAppointments.GroupBy(s => s.ScopeId).ToList();
                foreach (var item in distinctSoa)
                {
                    var scopeOfApps = legislativeAreaViewModel.ScopeOfAppointments;
                    scopeOfApps.First(soa => soa.ScopeId == item.Key).NoOfProductsInScopeOfAppointment = scopeOfApps.Count(soa => soa.ScopeId == item.Key);
                }

                if (legislativeAreaViewModel.IsArchived == true)
                {
                    viewModel.ArchivedLegislativeAreas.Add(legislativeAreaViewModel);
                }
                else
                {
                    viewModel.ActiveLegislativeAreas.Add(legislativeAreaViewModel);
                }

            }

            return viewModel;
        }

        private void ValidateCabSummaryModels(CABDetailsViewModel cabDetails, CABContactViewModel cabContact,
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

        private int LAPendingApprovalCountForUser(Document document, bool isOpssAdmin = false)
        {
            if (!isOpssAdmin)
            {
                return _legislativeAreaDetailService.GetPendingApprovalDocumentLegislativeAreaList(document, User).Count;
            }   

            return document.DocumentLegislativeAreas.Count(dla => dla.Status is LAStatus.Approved or LAStatus.PendingApprovalToRemoveByOpssAdmin or LAStatus.PendingApprovalToToArchiveAndArchiveScheduleByOpssAdmin or LAStatus.PendingApprovalToToArchiveAndRemoveScheduleByOpssAdmin);
        }
    }
}