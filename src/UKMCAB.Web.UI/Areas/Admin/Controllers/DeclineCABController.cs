using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Notify.Interfaces;
using UKMCAB.Common.Exceptions;
using UKMCAB.Core;
using UKMCAB.Core.Domain.Workflow;
using UKMCAB.Core.EmailTemplateOptions;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Core.Services.Workflow;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.Users;
using UKMCAB.Web.UI.Areas.Search.Controllers;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers;

[Area("admin"), Route("admin/cab/decline"), Authorize]
public class DeclineCABController : Controller
{
    private readonly ICABAdminService _cabAdminService;
    private readonly IUserService _userService;
    private readonly IAsyncNotificationClient _notificationClient;
    private readonly CoreEmailTemplateOptions _templateOptions;
    private readonly IWorkflowTaskService _workflowTaskService;

    public DeclineCABController(
        ICABAdminService cabAdminService,
        IUserService userService,
        IAsyncNotificationClient notificationClient,
        IOptions<CoreEmailTemplateOptions> templateOptions,
        IWorkflowTaskService workflowTaskService)
    {
        _cabAdminService = cabAdminService;
        _userService = userService;
        _notificationClient = notificationClient;
        _workflowTaskService = workflowTaskService;
        _templateOptions = templateOptions.Value;
    }

    public static class Routes
    {
        public const string Decline = "cab.decline";
    }

    [HttpGet("{cabId}", Name = Routes.Decline)]
    public async Task<IActionResult> DeclineAsync(string cabId)
    {
        var document = await _cabAdminService.GetLatestDocumentAsync(cabId) ??
                       throw new InvalidOperationException("CAB not found");
        if (document.StatusValue != Status.Draft || document.SubStatus != SubStatus.PendingApprovalToPublish)
        {
            throw new PermissionDeniedException("CAB status needs to be Pending Approval for decline");
        }

        var model = new DeclineCABViewModel("Decline CAB", document.Name ?? throw new InvalidOperationException());
        return View("~/Areas/Admin/Views/CAB/Decline.cshtml", model);
    }

    [HttpPost("{cabId}")]
    public async Task<IActionResult> DeclinePostAsync(Guid cabId,
        [Bind(nameof(DeclineCABViewModel.DeclineReason))]
        DeclineCABViewModel vm)
    {
        var document = await _cabAdminService.GetLatestDocumentAsync(cabId.ToString()) ??
                       throw new InvalidOperationException("CAB not found");
        ModelState.Remove(nameof(DeclineCABViewModel.CABName));
        if (!ModelState.IsValid)
        {
            vm.CABName = document.Name ?? throw new InvalidOperationException();
            vm.Title = "Decline CAB";
            return View("~/Areas/Admin/Views/CAB/Decline.cshtml", vm);
        }

        var user =
            await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value) ??
            throw new InvalidOperationException();
        var userRoleId = Roles.List.First(r =>
            r.Label != null && r.Label.Equals(user.Role, StringComparison.CurrentCultureIgnoreCase)).Id;
        await _cabAdminService.SetSubStatusAsync(cabId, Status.Draft, SubStatus.None,
            new Audit(user, AuditCABActions.CABDeclined, vm.DeclineReason));

        var submitTask = await MarkTaskAsCompleteAsync(cabId,
            new User(user.Id, user.FirstName, user.Surname, userRoleId,
                user.EmailAddress ?? throw new InvalidOperationException()));
        await SendNotificationOfDeclineAsync(cabId, document.Name, submitTask.Submitter, vm.DeclineReason);
        return RedirectToRoute(CabManagementController.Routes.CABManagement);
    }

    /// <summary>
    /// Mark incoming Request to publish task as completed
    /// </summary>
    /// <param name="cabId">Associated CAB</param>
    /// <param name="userLastUpdatedBy"></param>
    private async Task<WorkflowTask> MarkTaskAsCompleteAsync(Guid cabId, User userLastUpdatedBy)
    {
        var tasks = await _workflowTaskService.GetByCabIdAsync(cabId);
        var task = tasks.First(t => t is { TaskType: TaskType.RequestToPublish, Completed: false });
        await _workflowTaskService.MarkTaskAsCompletedAsync(task.Id, userLastUpdatedBy);
        return task;
    }

    /// <summary>
    /// Sends an email and notification for declined cab
    /// </summary>
    /// <param name="cabId">CAB id</param>
    /// <param name="cabName">Name of CAB</param>
    /// <param name="submitter"></param>
    /// <param name="declineReason"></param>
    private async Task SendNotificationOfDeclineAsync(Guid cabId, string? cabName, User submitter, string declineReason)
    {
        if (cabName == null) throw new ArgumentNullException(nameof(cabName));
        var personalisation = new Dictionary<string, dynamic?>
        {
            { "CABName", cabName },
            {
                "CABUrl",
                UriHelper.GetAbsoluteUriFromRequestAndPath(HttpContext.Request,
                    Url.RouteUrl(CABController.Routes.CabSummary, new { id = cabId }))
            },
            { "DeclineReason", declineReason }
        };
        await _notificationClient.SendEmailAsync(submitter.EmailAddress,
            _templateOptions.NotificationCabDeclined, personalisation);
        var user =
            await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value) ??
            throw new InvalidOperationException();
        var approver = new User(user.Id, user.FirstName, user.Surname,
            user.Role ?? throw new InvalidOperationException(),
            user.EmailAddress ?? throw new InvalidOperationException());
        await _workflowTaskService.CreateAsync(
            new WorkflowTask(
                TaskType.CABDeclined,
                approver,
                // Approver becomes the submitter for Declined CAB Notification
                submitter.RoleId,
                submitter,
                DateTime.Now,
                $"The request to approve CAB {cabName} has been declined for the following reason: {declineReason}.",
                approver,
                DateTime.Now,
                false,
                declineReason,
                true,
                cabId));
    }
}