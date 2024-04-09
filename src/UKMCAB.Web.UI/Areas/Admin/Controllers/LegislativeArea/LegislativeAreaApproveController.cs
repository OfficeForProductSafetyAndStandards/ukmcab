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
        
        var lasToApprove =
            UserRoleId == Roles.OPSS.Id ? document.DocumentLegislativeAreas.Where(la => la.Status == LAStatus.Approved).ToList() :
                document.DocumentLegislativeAreas.Where(la => la.Status == LAStatus.PendingApproval && la.RoleId == UserRoleId).ToList();
        if (!lasToApprove.Any())
        {
            return RedirectToRoute(CABController.Routes.CabSummary, new { id, subSectionEditAllowed = true });
        }

        var las = await GetLegislativeAreasForUserAsync();
        var vm = new ApprovalListViewModel() { CabId = id };

        foreach (var dla in lasToApprove)
        {
            var laName = las.Single(l => l.Id == dla.LegislativeAreaId).Name;
            vm.LasToApprove.Add(new(dla.LegislativeAreaId, laName));
        }
        
        //todo: need to clear temp data for success message

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

        var la = await _legislativeAreaService.GetLegislativeAreaByIdAsync(legislativeAreaId);

        var vm = new LegislativeAreaApproveViewModel
        {   
            CabId = id,
            Title = "Approve legislative area",
            LegislativeArea = await _legislativeAreaDetailService.PopulateCABLegislativeAreasItemViewModelAsync(latestDocument, legislativeAreaId),
            ActiveProductSchedules = latestDocument.ActiveSchedules?.Where(n => n.LegislativeArea == la.Name).ToList(),
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
        
        if (ModelState.IsValid)
        {           
            if(vm.LegislativeAreaApproveActionEnum == LegislativeAreaApproveActionEnum.Approve)
            {
                await ApproveLegislativeAreaAsync(documentLa, latestDocument);
            }
            else
            {
                await DeclineLegislativeAreaAsync(documentLa, latestDocument, vm.DeclineReason);
            }

            return RedirectToRoute(Routes.LegislativeAreaApprovalList, new { id });
        }
        else
        {
            vm.LegislativeArea = await _legislativeAreaDetailService.PopulateCABLegislativeAreasItemViewModelAsync(latestDocument, legislativeAreaId);
            vm.ActiveProductSchedules = latestDocument.ActiveSchedules.Where(n => n.LegislativeArea == documentLa.LegislativeAreaName).ToList();
        }      

        return View("~/Areas/Admin/views/CAB/LegislativeArea/ApproveDeclineLegislativeAreaSelection.cshtml", vm);
    }

    [HttpPost("approve", Name = Routes.LegislativeAreaApprove)]
    public async Task<IActionResult> ApprovePostAsync(Guid id)
    {
        var document = await _cabAdminService.GetLatestDocumentAsync(id.ToString()) ??
                       throw new InvalidOperationException("CAB not found");
        
        var docLasPendingApproval =
            document.DocumentLegislativeAreas.Where(l =>
                l.Status == LAStatus.PendingApproval && l.RoleId == UserRoleId).ToList();
        if (!docLasPendingApproval.Any())
        {
            throw new PermissionDeniedException("No legislative area for approval on CAB for this OGD");
        }

        var docLa = docLasPendingApproval.First();
        await ApproveLegislativeAreaAsync(docLa, document);
        return RedirectToRoute(CABController.Routes.CabSummary, new { id, subSectionEditAllowed = true });
    }

    [HttpGet("decline", Name = Routes.LegislativeAreaDecline)]
    public async Task<IActionResult> DeclineAsync(Guid id)
    {
        var la = (await GetLegislativeAreasForUserAsync()).First(); //todo multiples incoming for OPSS OGD
        var vm = new DeclineLAViewModel($"Decline Legislative area {la.Name}", id);
        return View("~/Areas/Admin/views/CAB/LegislativeArea/Decline.cshtml", vm);
    }

    [HttpPost("decline")]
    public async Task<IActionResult> DeclinePostAsync(Guid id,
        [Bind(nameof(DeclineLAViewModel.DeclineReason))]
        DeclineLAViewModel vm)
    {
        var document = await _cabAdminService.GetLatestDocumentAsync(id.ToString()) ??
                       throw new InvalidOperationException("CAB not found");
        var docLasPendingApproval =
            document.DocumentLegislativeAreas.Where(l =>
                l.Status == LAStatus.PendingApproval && l.RoleId == UserRoleId).ToList();
        if (!docLasPendingApproval.Any())
        {
            throw new PermissionDeniedException("No legislative area for approval on CAB for this OGD");
        }

        var docLa = docLasPendingApproval.First();

        if (ModelState.IsValid)
        {
            await DeclineLegislativeAreaAsync(docLa, document, vm.DeclineReason);
            return RedirectToRoute(CABController.Routes.CabSummary, new { id, subSectionEditAllowed = true });
        }

        var viewModel = new DeclineLAViewModel($"Decline Legislative area {docLa.LegislativeAreaName}", id);
        vm.DeclineReason = vm.DeclineReason;
        return View("~/Areas/Admin/views/CAB/LegislativeArea/Decline.cshtml", viewModel);
    }

    private async Task ApproveLegislativeAreaAsync(DocumentLegislativeArea docLa, Document document)
    {
        var currentUser = CurrentUser;
        var approver = new User(currentUser.Id, currentUser.FirstName, currentUser.Surname,
            UserRoleId ?? throw new InvalidOperationException(),
            currentUser.EmailAddress ?? throw new InvalidOperationException());

        var cabId = new Guid(document.CABId);
        await _cabAdminService.ApproveLegislativeAreaAsync((await _userService.GetAsync(User.GetUserId()!))!, cabId, docLa.LegislativeAreaId);
        TempData.Add(Constants.ApprovedLA, true);

        await MarkLARequestTaskAsCompleteAsync(cabId, approver);
        await SendNotificationOfLegislativeAreaApprovalAsync(cabId, document.Name, docLa.LegislativeAreaName, currentUser);
    }

    private async Task DeclineLegislativeAreaAsync(DocumentLegislativeArea docLa, Document document, string? declineReason)
    { 
        var cabId = new Guid(document.CABId);
        declineReason ??= string.Empty;
        await _cabAdminService.DeclineLegislativeAreaAsync((await _userService.GetAsync(User.GetUserId()!))!, cabId, docLa.LegislativeAreaId, declineReason);
        if (TempData.ContainsKey(Constants.DeclinedLA))
        {
            TempData.Remove(Constants.DeclinedLA);
            TempData.Add(Constants.DeclinedLA, true);
        }

        // send legislative area decline notification
        await SendNotificationOfDeclineAsync(cabId, document.Name, docLa.LegislativeAreaName, declineReason);
    }

    private async Task<IList<LegislativeAreaModel>> GetLegislativeAreasForUserAsync()
    {
        return UserRoleId != Roles.OPSS.Id 
            ? (await _legislativeAreaService.GetLegislativeAreasByRoleId(UserRoleId)).ToList() 
            : (await _legislativeAreaService.GetAllLegislativeAreasAsync()).ToList();
    }

    private async Task SendNotificationOfLegislativeAreaApprovalAsync(Guid cabId, string cabName, string legislativeAreaName, UserAccount approver)
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
                { "legislativeAreaName", legislativeAreaName }
            };

        await _notificationClient.SendEmailAsync(_templateOptions.ApprovedBodiesEmail,
            _templateOptions.NotificationLegislativeAreaApproved, personalisation);

        await _workflowTaskService.CreateAsync(
            new WorkflowTask(
                TaskType.LegislativeAreaApproved,
                approverUser,
                Roles.OPSS.Id,
                null,
                DateTime.Now,
                $"{approverUser.FirstAndLastName} from {approverUser.UserGroup} has approved the {legislativeAreaName} legislative area.",
                approverUser,
                DateTime.Now,
                true,
                null,
                true,
                cabId));
    }

    /// <summary>
    /// Sends an email and notification for declined legislative area
    /// </summary>
    /// <param name="cabId">CAB id</param>
    /// <param name="cabName">Name of CAB</param>
    /// <param name="legislativeAreaName">Name of legislative area</param>
    /// <param name="declineReason">Decline reason</param>
    private async Task SendNotificationOfDeclineAsync(Guid cabId, string? cabName, string legislativeAreaName, string? declineReason)
    {
        if (cabName == null) throw new ArgumentNullException(nameof(cabName));

        var approver = new User(CurrentUser.Id, CurrentUser.FirstName, CurrentUser.Surname,
            CurrentUser.Role ?? throw new InvalidOperationException(),
            CurrentUser.EmailAddress ?? throw new InvalidOperationException());

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
            { "legislativeAreaName", legislativeAreaName }
        };

        // send email to submitter group email 
        await _notificationClient.SendEmailAsync(_templateOptions.UkasGroupEmail,
            _templateOptions.NotificationLegislativeAreaDeclined, personalisation);

        await _workflowTaskService.CreateAsync(
            new WorkflowTask(
                TaskType.LegislativeAreaDeclined,
                approver,
                Roles.UKAS.Id,
                null,
                DateTime.Now,
                $"{approver.FirstAndLastName} from {approver.UserGroup} has declined the {legislativeAreaName} legislative area.",
                approver,
                DateTime.Now,
                false,
                declineReason,
                false,
                cabId));
    }

    private async Task<WorkflowTask> MarkLARequestTaskAsCompleteAsync(Guid documentLAId, User approver)
    {
        var tasks = await _workflowTaskService.GetByDocumentLAIdAsync(documentLAId);
        var task = 
            approver.RoleId != Roles.OPSS.Id ?
                tasks.First(t =>
                    t.TaskType is TaskType.LegislativeAreaApproveRequestForCab && t.ForRoleId == approver.RoleId && !t.Completed)
                : tasks.First(t => t.TaskType is TaskType.LegislativeAreaApproved && !t.Completed);
        await _workflowTaskService.MarkTaskAsCompletedAsync(task.Id, approver);
        return task;
    }
}