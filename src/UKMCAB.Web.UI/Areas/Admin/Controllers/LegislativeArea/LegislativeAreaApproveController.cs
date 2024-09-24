using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Notify.Interfaces;
using UKMCAB.Common.Exceptions;
using UKMCAB.Core.Domain.LegislativeAreas;
using UKMCAB.Core.Domain.Workflow;
using UKMCAB.Core.EmailTemplateOptions;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Core.Services.Workflow;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.Users;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Enums;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;
using UKMCAB.Web.UI.Services;
using UKMCAB.Common.Extensions;
using System.Globalization;
using UKMCAB.Subscriptions.Core.Common;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;
using AngleSharp.Common;
using UKMCAB.Core.Extensions;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers.LegislativeArea;

[Area("admin"), Route("admin/cab/{id}/legislative-area/"), Authorize(Claims.LegislativeAreaApprove)]
public class LegislativeAreaApproveController : UI.Controllers.ControllerBase
{
    private readonly ICABAdminService _cabAdminService;
    private readonly ILegislativeAreaService _legislativeAreaService;
    private readonly IAsyncNotificationClient _notificationClient;
    private readonly CoreEmailTemplateOptions _templateOptions;
    private readonly IWorkflowTaskService _workflowTaskService;
    private readonly ILegislativeAreaDetailService _legislativeAreaDetailService;

    public static class Routes
    {
        public const string LegislativeAreaApprovalList = "legislative.area.approval.list";
        public const string LegislativeAreaApprove = "legislative.area.approve";
        public const string LegislativeAreaApproveDeclineSelection = "legislative.area.approve.decline.selection";
        public const string LegislativeAreaDecline = "legislative.area.decline";
        public const string LegislativeAreaDeclineReason = "legislative.area.decline.reason";
    }

    public LegislativeAreaApproveController(ICABAdminService cabAdminService,
        ILegislativeAreaService legislativeAreaService,
        IUserService userService,
        ILegislativeAreaDetailService legislativeAreaDetailService,
        IAsyncNotificationClient notificationClient,
        IOptions<CoreEmailTemplateOptions> templateOptions,
        IWorkflowTaskService workflowTaskService) : base(userService)
    {
        _cabAdminService = cabAdminService;
        _legislativeAreaService = legislativeAreaService;
        _notificationClient = notificationClient;
        _templateOptions = templateOptions.Value;
        _workflowTaskService = workflowTaskService;
        _legislativeAreaDetailService = legislativeAreaDetailService;
    }

    [HttpGet("approvallist", Name = Routes.LegislativeAreaApprovalList)]
    public async Task<IActionResult> ApprovalListAsync(Guid id)
    {
        var document = await _cabAdminService.GetLatestDocumentAsync(id.ToString()) ??
                       throw new InvalidOperationException("CAB not found");
        
        var lasToApprove = UserRoleId == Roles.OPSS.Id 
            ? document.DocumentLegislativeAreas.Where(la => 
                la.Status is LAStatus.Approved or 
                LAStatus.PendingApprovalToRemoveByOpssAdmin or 
                LAStatus.PendingApprovalToArchiveAndArchiveScheduleByOpssAdmin or 
                LAStatus.PendingApprovalToArchiveAndRemoveScheduleByOpssAdmin or
                LAStatus.PendingApprovalToUnarchiveByOpssAdmin).ToList() 
            : _legislativeAreaDetailService.GetPendingApprovalDocumentLegislativeAreaList(document, User);

        if (!lasToApprove.Any())
        {
            return RedirectToRoute(CABController.Routes.CabSummary, new { id, revealEditActions = true });
        }

        var las = await GetLegislativeAreasForUserAsync();
        var vm = new ApprovalListViewModel { CabId = id };

        foreach (var dla in lasToApprove)
        {
            var laName = las.Single(l => l.Id == dla.LegislativeAreaId).Name;
            vm.LasToApprove.Add(new(dla.LegislativeAreaId, laName));
        }
        
        ShowSuccessMessage(vm);

        return View("~/Areas/Admin/views/CAB/LegislativeArea/ApprovalList.cshtml", vm);
    }   

    [HttpGet("approve-decline-selection/{legislativeAreaId}", Name = Routes.LegislativeAreaApproveDeclineSelection)]
    public async Task<IActionResult> ApproveDeclineSelection(Guid id, Guid legislativeAreaId)
    {
        var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString()) ??
                       throw new InvalidOperationException("CAB not found");
        
        if (!latestDocument.DocumentLegislativeAreas.Select(l => l.LegislativeAreaId).Contains(legislativeAreaId))
        {
            throw new PermissionDeniedException("No legislative area on CAB");
        }

        var documentLa = latestDocument.DocumentLegislativeAreas
           .First(l => l.LegislativeAreaId == legislativeAreaId);
        var la = await _legislativeAreaService.GetLegislativeAreaByIdAsync(legislativeAreaId);

        var reviewAction = documentLa.Status switch
        {   
            LAStatus.PendingApprovalToRemove or LAStatus.PendingApprovalToRemoveByOpssAdmin => LegislativeAreaReviewActionEnum.Remove,
            LAStatus.PendingApprovalToArchiveAndArchiveSchedule or LAStatus.PendingApprovalToArchiveAndArchiveScheduleByOpssAdmin => LegislativeAreaReviewActionEnum.ArchiveAndArchiveSchedule,
            LAStatus.PendingApprovalToArchiveAndRemoveSchedule or LAStatus.PendingApprovalToArchiveAndRemoveScheduleByOpssAdmin => LegislativeAreaReviewActionEnum.ArchiveAndRemoveSchedule,
            LAStatus.PendingApprovalToUnarchive or LAStatus.PendingApprovalToUnarchiveByOpssAdmin => LegislativeAreaReviewActionEnum.Unarchive,
            _ => LegislativeAreaReviewActionEnum.Add,
        };

        var vm = new LegislativeAreaApproveViewModel
        {
            CabId = id,           
            Title = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(reviewAction.GetEnumDescription())} legislative area",
            LegislativeArea = await _legislativeAreaDetailService.PopulateCABLegislativeAreasItemViewModelAsync(latestDocument, legislativeAreaId),
            ProductSchedules = reviewAction == LegislativeAreaReviewActionEnum.Unarchive ?
                latestDocument.ArchivedSchedules.Where(n => n.LegislativeArea == la.Name).ToList() :
                latestDocument.ActiveSchedules.Where(n => n.LegislativeArea == la.Name).ToList(),
            ReviewActionEnum = reviewAction
        };

        return View("~/Areas/Admin/views/CAB/LegislativeArea/ApproveDeclineLegislativeAreaSelection.cshtml", vm);
    }

    [HttpPost("approve-decline-selection/{legislativeAreaId}", Name = Routes.LegislativeAreaApproveDeclineSelection)]
    public async Task<IActionResult> ApproveDeclineSelection(Guid id, Guid legislativeAreaId, LegislativeAreaApproveViewModel vm)
    {
        var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString()) ??
                       throw new InvalidOperationException("CAB not found");

        if (!latestDocument.DocumentLegislativeAreas.Select(l => l.LegislativeAreaId).Contains(legislativeAreaId))
        {
            throw new PermissionDeniedException("No legislative area on CAB");
        }

        var documentLa = latestDocument.DocumentLegislativeAreas
            .First(l => l.LegislativeAreaId == legislativeAreaId);

        var userAccount = await User.GetUserId().MapAsync(x => _userService.GetAsync(x!));

        var publishModel = new CABSummaryViewModel
        {
            CABId = latestDocument.CABId,
            CabDetailsViewModel = new CABDetailsViewModel(latestDocument, User),
            CabContactViewModel = new CABContactViewModel(latestDocument),
            CabBodyDetailsMRAViewModel = new CABBodyDetailsMRAViewModel(latestDocument),
            CABProductScheduleDetailsViewModel = new CABProductScheduleDetailsViewModel(latestDocument),
            CABSupportingDocumentDetailsViewModel = new CABSupportingDocumentDetailsViewModel(latestDocument)
        };

        if (ModelState.IsValid)
        {
            if (vm.LegislativeAreaApproveActionEnum == LegislativeAreaApproveActionEnum.Approve)
            {
                await ApproveLegislativeAreaAsync(documentLa, latestDocument, vm.ReviewActionEnum);
                await SendNotificationForApproveCab(userAccount, latestDocument.Name ?? throw new InvalidOperationException(), publishModel);

                var legislativeAreaSenderEmailIds =
                        _templateOptions.NotificationLegislativeAreaEmails.ToDictionary();
                var receiverEmailId = legislativeAreaSenderEmailIds[documentLa.RoleId];

                await SendEmailNotificationOfLegislativeAreaApprovalAsync(Guid.Parse(latestDocument.CABId),
                    latestDocument.Name, userAccount, receiverEmailId, documentLa.LegislativeAreaName, 1);
            }
            else
            {
                return RedirectToRoute(Routes.LegislativeAreaDeclineReason, new { id, legislativeAreaId, vm.ReviewActionEnum });
            }

            return RedirectToRoute(Routes.LegislativeAreaApprovalList, new { id });
        }
        else
        {
            vm.LegislativeArea = await _legislativeAreaDetailService.PopulateCABLegislativeAreasItemViewModelAsync(latestDocument, legislativeAreaId);
            vm.ProductSchedules = vm.ReviewActionEnum == LegislativeAreaReviewActionEnum.Unarchive ?
                latestDocument.ArchivedSchedules.Where(n => n.LegislativeArea == documentLa.LegislativeAreaName).ToList() :
                latestDocument.ActiveSchedules.Where(n => n.LegislativeArea == documentLa.LegislativeAreaName).ToList();
        }

        return View("~/Areas/Admin/views/CAB/LegislativeArea/ApproveDeclineLegislativeAreaSelection.cshtml", vm);
    }    
    

    [HttpGet("decline-reason/{legislativeAreaId}/{ReviewActionEnum}", Name = Routes.LegislativeAreaDeclineReason)]
    public async Task<IActionResult> DeclineReason(Guid id, Guid legislativeAreaId, LegislativeAreaReviewActionEnum ReviewActionEnum)
    {
        var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString()) ??
                       throw new InvalidOperationException("CAB not found");

        var documentLa = latestDocument.DocumentLegislativeAreas
            .FirstOrDefault(l => l.LegislativeAreaId == legislativeAreaId);

        if (documentLa == null)
        {
            throw new PermissionDeniedException("No legislative area on CAB");
        }        

        var vm = new LegislativeAreaDeclineReasonViewModel
        {
            CabId = id,
            Title = "Decline legislative area",
            LegislativeAreaId = legislativeAreaId,
            LegislativeAreaName = documentLa.LegislativeAreaName,
            ReviewActionEnum = ReviewActionEnum
        };

        return View("~/Areas/Admin/views/CAB/LegislativeArea/DeclineLegislativeAreaReason.cshtml", vm);
    }

    [HttpPost("decline-reason/{legislativeAreaId}/{ReviewActionEnum}", Name = Routes.LegislativeAreaDeclineReason)]
    public async Task<IActionResult> DeclineReason(Guid id, Guid legislativeAreaId,  LegislativeAreaDeclineReasonViewModel vm)
    {
        var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString()) ??
                       throw new InvalidOperationException("CAB not found");

        if (!latestDocument.DocumentLegislativeAreas.Select(l => l.LegislativeAreaId).Contains(legislativeAreaId))
        {
            throw new PermissionDeniedException("No legislative area on CAB");
        }

        var documentLa = latestDocument.DocumentLegislativeAreas
            .First(l => l.LegislativeAreaId == legislativeAreaId);

        if (ModelState.IsValid)
        {
            await DeclineLegislativeAreaAsync(documentLa, latestDocument, vm.ReviewActionEnum, vm.DeclineReason);
            return RedirectToRoute(Routes.LegislativeAreaApprovalList, new { id });
        }

        return View("~/Areas/Admin/views/CAB/LegislativeArea/DeclineLegislativeAreaReason.cshtml", vm);
    }

    private async Task SendEmailNotificationOfLegislativeAreaApprovalAsync(Guid cabId, string cabName,
            UserAccount userAccount, string legislativeAreaReceiverEmailId, string legislativeAreaName,
            int legislativeAreaCount)
    {
        var user = new User(userAccount.Id, userAccount.FirstName, userAccount.Surname,
            userAccount.Role ?? throw new InvalidOperationException(),
            userAccount.EmailAddress ?? throw new InvalidOperationException());

        var emailBody =
            $"{user.FirstAndLastName} from {user.UserGroup} has requested that the {legislativeAreaName} legislative area is approved for CAB [{cabName}]({UriHelper.GetAbsoluteUriFromRequestAndPath(HttpContext.Request, Url.RouteUrl(CABController.Routes.CabSummary, new { id = cabId }))}).";
        if (legislativeAreaCount > 1)
            emailBody =
                $"{user.FirstAndLastName} from {user.UserGroup} has requested that the following legislative areas are approved for CAB [{cabName}]({UriHelper.GetAbsoluteUriFromRequestAndPath(HttpContext.Request, Url.RouteUrl(CABController.Routes.CabSummary, new { id = cabId }))}) : {legislativeAreaName}.";

        var personalisation = new Dictionary<string, dynamic?>
            {
                { "CABName", cabName },
                { "emailBody", emailBody },
                { "userGroup", user.UserGroup }
            };
        await _notificationClient.SendEmailAsync(legislativeAreaReceiverEmailId,
            _templateOptions.NotificationLegislativeAreaRequestToPublish, personalisation);
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
        var tasks = await _workflowTaskService.GetByCabIdAsync(Guid.Parse(publishModel.CABId!));
        var requestToPublish = tasks.SingleOrDefault(t => t.TaskType == TaskType.RequestToPublish && !t.Completed);
        if (requestToPublish == null)
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
    }

    private void ShowSuccessMessage(ApprovalListViewModel vm)
    {
        if (TempData.ContainsKey(Constants.ApprovedLA))
        {
            TempData.Remove(Constants.ApprovedLA);
            vm.SuccessBannerMessage = "Legislative area has been approved.";
        }

        if (TempData.ContainsKey(Constants.DeclinedLA))
        {
            TempData.Remove(Constants.DeclinedLA);
            vm.SuccessBannerMessage = "Legislative area has been declined.";
        }
    }

    private async Task ApproveLegislativeAreaAsync(DocumentLegislativeArea docLa, Document document, LegislativeAreaReviewActionEnum ReviewActionEnum)
    {
        var currentUser = CurrentUser;
        var approver = new User(currentUser.Id, currentUser.FirstName, currentUser.Surname,
            UserRoleId ?? throw new InvalidOperationException(),
            currentUser.EmailAddress ?? throw new InvalidOperationException());

        var cabId = new Guid(document.CABId);
        
        var newLAStatus = GetNewLAStatusOnApprove(ReviewActionEnum);
        await _cabAdminService.ApproveLegislativeAreaAsync((await _userService.GetAsync(User.GetUserId()!))!, cabId, docLa.LegislativeAreaId, newLAStatus);
        TempData[Constants.ApprovedLA] = true;

        await MarkRequestTaskAsCompleteAsync(docLa.Id, approver);
        if (currentUser.Role != Roles.OPSS.Id)
        {
            await SendNotificationOfLegislativeAreaApprovalAsync(cabId, document.Name, docLa, currentUser,
                ReviewActionEnum, document.CreatedByUserGroup);
        }
    }

    private LAStatus GetNewLAStatusOnApprove(LegislativeAreaReviewActionEnum reviewActionEnum)
    {
        var newLAStatus = LAStatus.Approved;
        if (UserRoleId != Roles.OPSS.Id)
        {
            newLAStatus = reviewActionEnum switch
            {
                LegislativeAreaReviewActionEnum.Add => LAStatus.Approved,
                LegislativeAreaReviewActionEnum.Remove => LAStatus.PendingApprovalToRemoveByOpssAdmin,
                LegislativeAreaReviewActionEnum.ArchiveAndArchiveSchedule => LAStatus.PendingApprovalToArchiveAndArchiveScheduleByOpssAdmin,
                LegislativeAreaReviewActionEnum.ArchiveAndRemoveSchedule => LAStatus.PendingApprovalToArchiveAndRemoveScheduleByOpssAdmin,
                LegislativeAreaReviewActionEnum.Unarchive => LAStatus.PendingApprovalToUnarchiveByOpssAdmin,
                _ => newLAStatus
            };
        }
        else
        {
            newLAStatus = reviewActionEnum switch
            {
                LegislativeAreaReviewActionEnum.Add => LAStatus.ApprovedByOpssAdmin,
                LegislativeAreaReviewActionEnum.Remove => LAStatus.ApprovedToRemoveByOpssAdmin,
                LegislativeAreaReviewActionEnum.ArchiveAndArchiveSchedule => LAStatus.ApprovedToArchiveAndArchiveScheduleByOpssAdmin,
                LegislativeAreaReviewActionEnum.ArchiveAndRemoveSchedule => LAStatus.ApprovedToArchiveAndRemoveScheduleByOpssAdmin,
                LegislativeAreaReviewActionEnum.Unarchive => LAStatus.ApprovedToUnarchiveByOPSS,
                _ => newLAStatus
            };
        }

        return newLAStatus;
    }

    private async Task DeclineLegislativeAreaAsync(DocumentLegislativeArea docLa, Document document, LegislativeAreaReviewActionEnum ReviewActionEnum, string? declineReason)
    {
        var currentUser = CurrentUser;
        var decliner = new User(currentUser.Id, currentUser.FirstName, currentUser.Surname,
            UserRoleId ?? throw new InvalidOperationException(),
            currentUser.EmailAddress ?? throw new InvalidOperationException());

        var cabId = new Guid(document.CABId);

        var newLAStatus = GetNewLAStatusOnDecline(ReviewActionEnum);
        declineReason ??= string.Empty;
        await _cabAdminService.DeclineLegislativeAreaAsync((await _userService.GetAsync(User.GetUserId()!))!, cabId, docLa.LegislativeAreaId, declineReason, newLAStatus);
        TempData[Constants.DeclinedLA] = true;

        await MarkRequestTaskAsCompleteAsync(docLa.Id, decliner);
        await SendNotificationOfDeclineAsync(cabId, document.Name, docLa, ReviewActionEnum, declineReason, document.CreatedByUserGroup);
    }

    private LAStatus GetNewLAStatusOnDecline(LegislativeAreaReviewActionEnum reviewActionEnum)
    {
        var newLAStatus = LAStatus.Declined;
        if (UserRoleId != Roles.OPSS.Id)
        {
            newLAStatus = reviewActionEnum switch
            {
                LegislativeAreaReviewActionEnum.Add => LAStatus.Declined,
                LegislativeAreaReviewActionEnum.Remove => LAStatus.DeclinedToRemoveByOGD,
                LegislativeAreaReviewActionEnum.ArchiveAndArchiveSchedule => LAStatus.DeclinedToArchiveAndArchiveScheduleByOGD,
                LegislativeAreaReviewActionEnum.ArchiveAndRemoveSchedule => LAStatus.DeclinedToArchiveAndRemoveScheduleByOGD,
                LegislativeAreaReviewActionEnum.Unarchive => LAStatus.DeclinedToUnarchiveByOGD,
                _ => newLAStatus
            };
        }
        else
        {
            newLAStatus = reviewActionEnum switch
            {
                LegislativeAreaReviewActionEnum.Add => LAStatus.DeclinedByOpssAdmin,
                LegislativeAreaReviewActionEnum.Remove => LAStatus.DeclinedToRemoveByOPSS,
                LegislativeAreaReviewActionEnum.ArchiveAndArchiveSchedule => LAStatus.DeclinedToArchiveAndArchiveScheduleByOPSS,
                LegislativeAreaReviewActionEnum.ArchiveAndRemoveSchedule => LAStatus.DeclinedToArchiveAndRemoveScheduleByOPSS,
                LegislativeAreaReviewActionEnum.Unarchive => LAStatus.DeclinedToUnarchiveByOPSS,
                _ => newLAStatus
            };
        }

        return newLAStatus;
    }

    private async Task<IList<LegislativeAreaModel>> GetLegislativeAreasForUserAsync()
    {
        return UserRoleId != Roles.OPSS.Id 
            ? (await _legislativeAreaService.GetLegislativeAreasByRoleId(UserRoleId)).ToList() 
            : (await _legislativeAreaService.GetAllLegislativeAreasAsync()).ToList();
    }

    private async Task SendNotificationOfLegislativeAreaApprovalAsync(Guid cabId, string cabName, DocumentLegislativeArea docLa, UserAccount approver, LegislativeAreaReviewActionEnum ReviewActionEnum, string createdByUserGroup)
    {
        var approverUser = new User(approver.Id, approver.FirstName, approver.Surname,
            approver.Role ?? throw new InvalidOperationException(),
            approver.EmailAddress ?? throw new InvalidOperationException());

        var personalisation = new Dictionary<string, dynamic?>
            {
                { "CABName", cabName },
                { "CABUrl",
                    UriHelper.GetAbsoluteUriFromRequestAndPath(HttpContext.Request,
                        Url.RouteUrl(CABController.Routes.CabSummary, new { id = cabId }))
                },
                { "userGroup", approverUser.UserGroup },
                { "userName", approverUser.FirstAndLastName },
                { "legislativeAreaName", docLa.LegislativeAreaName },
                { "action" , ReviewActionEnum.GetEnumDescription() }              
            };

        string? workFlowBody;
        TaskType taskType = TaskType.LegislativeAreaApproveRequestForCab;

        // if add LA
        if (ReviewActionEnum == LegislativeAreaReviewActionEnum.Add)
        {
            // send email to opss admin
            await _notificationClient.SendEmailAsync(_templateOptions.ApprovedBodiesEmail,
                _templateOptions.NotificationLegislativeAreaPublishApproved, personalisation);

            workFlowBody = $"{approverUser.FirstAndLastName} from {approverUser.UserGroup} has approved the {docLa.LegislativeAreaName} legislative area";
        }
        // if remove/archive/unarchive LA
        else 
        {
            // send email to submitter group email 

            taskType = ReviewActionEnum switch
            {
                LegislativeAreaReviewActionEnum.Remove => TaskType.LegislativeAreaRequestToRemove,
                LegislativeAreaReviewActionEnum.ArchiveAndArchiveSchedule => TaskType.LegislativeAreaRequestToArchiveAndArchiveSchedule,
                LegislativeAreaReviewActionEnum.ArchiveAndRemoveSchedule => TaskType.LegislativeAreaRequestToArchiveAndRemoveSchedule,
                LegislativeAreaReviewActionEnum.Unarchive => TaskType.LegislativeAreaRequestToUnarchive
            };
            
            personalisation.Add("Reason", docLa.RequestReason);

            await _notificationClient.SendEmailAsync(_templateOptions.ApprovedBodiesEmail,
                _templateOptions.NotificationLegislativeAreaToRemoveArchiveUnArchiveApproved, personalisation);

            workFlowBody = $"{approverUser.FirstAndLastName} from {approverUser.UserGroup} has approved the request to {ReviewActionEnum.GetEnumDescription()} the {docLa.LegislativeAreaName} legislative area.";
        }       

        await _workflowTaskService.CreateAsync(
            new WorkflowTask(
                taskType,
                approverUser,
                Roles.OPSS.Id,
                null,
                DateTime.Now,
                workFlowBody,
                approverUser,
                DateTime.Now,
                true,
                null,
                false,
                cabId,
                docLa.Id
                ));
    }

    /// <summary>
    /// Sends an email and notification for declined legislative area
    /// </summary>
    /// <param name="cabId">CAB id</param>
    /// <param name="cabName">Name of CAB</param>
    /// <param name="docLa">Document Legislative Area</param>
    /// <param name="ReviewActionEnum">Review action</param>
    /// <param name="declineReason">Decline reason</param>
    private async Task SendNotificationOfDeclineAsync(Guid cabId, string? cabName, DocumentLegislativeArea docLa, LegislativeAreaReviewActionEnum ReviewActionEnum, string? declineReason, string createdByUserGroup)
    {
        if (cabName == null) throw new ArgumentNullException(nameof(cabName));

        var approver = new User(CurrentUser.Id, CurrentUser.FirstName, CurrentUser.Surname,
            CurrentUser.Role ?? throw new InvalidOperationException(),
            CurrentUser.EmailAddress ?? throw new InvalidOperationException());

        string? workFlowBody;

        TaskType taskType = TaskType.LegislativeAreaDeclined;

        var personalisation = new Dictionary<string, dynamic?>
        {
            { "CABName", cabName },
            {
                "CABUrl",
                UriHelper.GetAbsoluteUriFromRequestAndPath(HttpContext.Request,
                    Url.RouteUrl(CABController.Routes.CabSummary, new { id = cabId }))
            },
            { "declineReason", declineReason },
            { "userGroup", approver.UserGroup },
            { "userName", approver.FirstAndLastName },
            { "legislativeAreaName", docLa.LegislativeAreaName },
            { "action" , ReviewActionEnum.GetEnumDescription() }
        };

        // if add LA
        if (ReviewActionEnum == LegislativeAreaReviewActionEnum.Add)
        {
            // send email to submitter group email 
            await _notificationClient.SendEmailAsync(_templateOptions.UkasGroupEmail,
                _templateOptions.NotificationLegislativeAreaPublishDeclined, personalisation);

            workFlowBody = $"{approver.FirstAndLastName} from {approver.UserGroup} has declined the {docLa.LegislativeAreaName} legislative area.";
        }
        // if remove/archive/unarchive LA
        else
        {
            // send email to submitter group email 
            await _notificationClient.SendEmailAsync(_templateOptions.UkasGroupEmail,
                _templateOptions.NotificationLegislativeAreaToRemoveArchiveUnArchiveDeclined, personalisation);

            workFlowBody = $"{approver.FirstAndLastName} from {approver.UserGroup} has declined the request to {ReviewActionEnum.GetEnumDescription()} the {docLa.LegislativeAreaName} legislative area.";

            taskType = docLa.Status switch
            {
                LAStatus.PendingApprovalToRemove or LAStatus.PendingApprovalToRemoveByOpssAdmin => TaskType.LegislativeAreaDeclinedToRemove,
                LAStatus.PendingApprovalToArchiveAndArchiveSchedule or LAStatus.PendingApprovalToArchiveAndRemoveSchedule or 
                    LAStatus.PendingApprovalToArchiveAndArchiveScheduleByOpssAdmin or LAStatus.PendingApprovalToArchiveAndRemoveScheduleByOpssAdmin => TaskType.LegislativeAreaDeclinedToArchive,
                LAStatus.PendingApprovalToUnarchive or LAStatus.PendingApprovalToUnarchiveByOpssAdmin => TaskType.LegislativeAreaDeclinedToUnarchive
            };
        }     

        await _workflowTaskService.CreateAsync(
            new WorkflowTask(
                taskType,
                approver,
                createdByUserGroup,
                null,
                DateTime.Now,
                workFlowBody,
                approver,
                DateTime.Now,
                false,
                declineReason,
                false,
                cabId,
                docLa.Id
                ));
    }

    private async Task MarkRequestTaskAsCompleteAsync(Guid documentLAId, User approver)
    {
        var tasks = await _workflowTaskService.GetByDocumentLAIdAsync(documentLAId);
        var task =
                tasks.First(t =>
                    t.TaskType is (TaskType.LegislativeAreaApproveRequestForCab or
                                   TaskType.LegislativeAreaRequestToRemove or
                                   TaskType.LegislativeAreaRequestToArchiveAndRemoveSchedule or
                                   TaskType.LegislativeAreaRequestToArchiveAndArchiveSchedule or
                                   TaskType.LegislativeAreaRequestToUnarchive) &&
                    t.ForRoleId == approver.RoleId && !t.Completed);
                
        await _workflowTaskService.MarkTaskAsCompletedAsync(task.Id, approver);
    }
}