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
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers.LegislativeArea;

[Area("admin"), Route("admin/cab/{id}/legislative-area/"), Authorize(Claims.LegislativeAreaApprove)]
public class LegislativeAreaApproveController : UI.Controllers.ControllerBase
{
    private readonly ICABAdminService _cabAdminService;
    private readonly ILegislativeAreaService _legislativeAreaService;
    private readonly IAsyncNotificationClient _notificationClient;
    private readonly CoreEmailTemplateOptions _templateOptions;
    private readonly IWorkflowTaskService _workflowTaskService;

    public static class Routes
    {
        public const string LegislativeAreaApprove = "legislative.area.approve";
    }

    public LegislativeAreaApproveController(ICABAdminService cabAdminService,
        ILegislativeAreaService legislativeAreaService,
        IUserService userService,
        IAsyncNotificationClient notificationClient,
        IOptions<CoreEmailTemplateOptions> templateOptions,
        IWorkflowTaskService workflowTaskService) : base(userService)
    {
        _cabAdminService = cabAdminService;
        _legislativeAreaService = legislativeAreaService;
        _notificationClient = notificationClient;
        _templateOptions = templateOptions.Value;
        _workflowTaskService = workflowTaskService;
    }

    [HttpPost("approve", Name = Routes.LegislativeAreaApprove)]
    public async Task<IActionResult> ApprovePostAsync(Guid id)
    {
        var document = await _cabAdminService.GetLatestDocumentAsync(id.ToString()) ??
                       throw new InvalidOperationException("CAB not found");
        var la = (await GetLegislativeAreasForUserAsync()).First(); //todo multiples incoming for OPSS OGD
        if (!document.DocumentLegislativeAreas.Select(l => l.LegislativeAreaId).Contains(la.Id))
        {
            throw new PermissionDeniedException("No legislative area on CAB owned by this OGD");
        }

        var currentUser = CurrentUser;
        var approver = new User(currentUser.Id, currentUser.FirstName, currentUser.Surname,
            UserRoleId ?? throw new InvalidOperationException(),
            currentUser.EmailAddress ?? throw new InvalidOperationException());

        await _cabAdminService.ApproveLegislativeAreaAsync((await _userService.GetAsync(User.GetUserId()!))!, id, la.Id);
        TempData.Add(Constants.ApprovedLA, true);

        await MarkTaskAsCompleteAsync(id, approver);
        await SendNotificationOfLegislativeAreaApprovalAsync(new Guid(document.CABId), document.Name!, la.Name, currentUser);

        return RedirectToRoute(CABController.Routes.CabSummary, new { id, subSectionEditAllowed = true });
    }

    private async Task<IList<LegislativeAreaModel>> GetLegislativeAreasForUserAsync()
    {
        return (await _legislativeAreaService.GetLegislativeAreasByRoleId(UserRoleId)).ToList();
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

    private async Task<WorkflowTask> MarkTaskAsCompleteAsync(Guid cabId, User userLastUpdatedBy)
    {
        var task = await GetWorkflowTaskAsync(cabId, userLastUpdatedBy.RoleId);
        await _workflowTaskService.MarkTaskAsCompletedAsync(task.Id, userLastUpdatedBy);
        return task;
    }

    private async Task<WorkflowTask> GetWorkflowTaskAsync(Guid cabId, string approverRoleId)
    {
        var tasks = await _workflowTaskService.GetByCabIdAsync(cabId);
        var task = tasks.First(t =>
            t.TaskType is TaskType.LegislativeAreaApproveRequestForCab && t.ForRoleId == approverRoleId && !t.Completed);
        return task;
    }
}