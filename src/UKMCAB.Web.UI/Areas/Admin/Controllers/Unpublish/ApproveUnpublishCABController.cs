using System.Security.Claims;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Notify.Interfaces;
using UKMCAB.Common.Exceptions;
using UKMCAB.Core.Domain.Workflow;
using UKMCAB.Core.EmailTemplateOptions;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Core.Services.Workflow;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.Users;
using UKMCAB.Web.UI.Areas.Search.Controllers;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Unpublish;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers.Unpublish;

[Area("admin"), Route("admin/cab/unpublish/approve"), Authorize(Policies.ApproveRequests)]
public class ApproveUnpublishCABController : Controller
{
    private readonly ICABAdminService _cabAdminService;
    private readonly IUserService _userService;
    private readonly IAsyncNotificationClient _notificationClient;
    private readonly CoreEmailTemplateOptions _templateOptions;
    private readonly IWorkflowTaskService _workflowTaskService;
    private readonly TelemetryClient _telemetryClient;

    public ApproveUnpublishCABController(
        ICABAdminService cabAdminService,
        IUserService userService,
        IAsyncNotificationClient notificationClient,
        IOptions<CoreEmailTemplateOptions> templateOptions,
        IWorkflowTaskService workflowTaskService,
        TelemetryClient telemetryClient)
    {
        _cabAdminService = cabAdminService;
        _userService = userService;
        _notificationClient = notificationClient;
        _workflowTaskService = workflowTaskService;
        _telemetryClient = telemetryClient;
        _templateOptions = templateOptions.Value;
    }

    public static class Routes
    {
        public const string Approve = "cab.unpublish.approve";
    }

    [HttpGet("{cabUrl}", Name = Routes.Approve)]
    public async Task<IActionResult> ApproveAsync(string cabUrl)
    {
        var document =
            (await _cabAdminService.FindAllDocumentsByCABURLAsync(cabUrl, new[] { Status.Published })).First();
        var unpublishStatuses = new List<SubStatus>()
        {
            SubStatus.PendingApprovalToUnpublish,
            SubStatus.PendingApprovalToArchive
        };
        if (document.StatusValue != Status.Published || !unpublishStatuses.Contains(document.SubStatus))
        {
            throw new PermissionDeniedException("CAB status needs to be Published and Submitted for approval");
        }

        var vm = new ApproveUnpublishCABViewModel(
            $"Unpublish CAB {document.Name}",
            document.Name ?? throw new InvalidOperationException(),
            document.URLSlug,
            Guid.Parse(document.CABId),
            document.SubStatus == SubStatus.PendingApprovalToArchive);

        return View("~/Areas/Admin/Views/CAB/unpublish/Approve.cshtml", vm);
    }

    [HttpPost("{cabUrl}", Name = Routes.Approve)]
    public async Task<IActionResult> ApprovePostAsync(string cabUrl, ApproveUnpublishCABViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Areas/Admin/Views/CAB/unpublish/Approve.cshtml", vm);
        }

        var currentUser = await _userService.GetAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)) ??
                          throw new InvalidOperationException();
        var userRoleId = Roles.List.First(r =>
            r.Label != null && r.Label.Equals(currentUser.Role, StringComparison.CurrentCultureIgnoreCase)).Id;

        var approver = new User(currentUser.Id, currentUser.FirstName, currentUser.Surname,
            userRoleId ?? throw new InvalidOperationException(),
            currentUser.EmailAddress ?? throw new InvalidOperationException());

        var task = await GetWorkflowTaskAsync(vm.CabId);
        var submitter = await _userService.GetAsync(task.Submitter.UserId);
        var document =
            (await _cabAdminService.FindAllDocumentsByCABURLAsync(cabUrl, new[] { Status.Published })).First();
        bool unpublishAndCreateDraft = task.TaskType == TaskType.RequestToUnpublish;
        if (unpublishAndCreateDraft)
        {
            var historical = await _cabAdminService.UnPublishDocumentAsync(currentUser, document.CABId, vm.Reason);
            await _cabAdminService.CreateDocumentAsync(submitter!, historical);
        }
        else
        {
            await _cabAdminService.ArchiveDocumentAsync(submitter!, vm.CabId.ToString(), vm.UserNotes, vm.Reason!);
        }

        var requestTask = await MarkTaskAsCompleteAsync(vm.CabId, approver);
        await SendNotificationOfApprovalAsync(vm.CabId, vm.CABName, requestTask.Submitter, approver,
            unpublishAndCreateDraft);
        return RedirectToRoute(CabManagementController.Routes.CABManagement);
    }

    private async Task<WorkflowTask> GetWorkflowTaskAsync(Guid cabId)
    {
        var tasks = await _workflowTaskService.GetByCabIdAsync(cabId);
        var task = tasks.First(t =>
            t.TaskType is TaskType.RequestToArchive or TaskType.RequestToUnpublish && !t.Completed);
        return task;
    }

    /// <summary>
    /// Mark incoming Request to unpublish task as completed
    /// </summary>
    /// <param name="cabId">Associated CAB</param>
    /// <param name="userLastUpdatedBy"></param>
    private async Task<WorkflowTask> MarkTaskAsCompleteAsync(Guid cabId, User userLastUpdatedBy)
    {
        var task = await GetWorkflowTaskAsync(cabId);
        await _workflowTaskService.MarkTaskAsCompletedAsync(task.Id, userLastUpdatedBy);
        return task;
    }

    /// <summary>
    /// Sends an email and notification for approved unpublish
    /// </summary>
    /// <param name="cabId">CAB id</param>
    /// <param name="cabName">Name of CAB</param>
    /// <param name="submitter"></param>
    /// <param name="approver"></param>
    /// <param name="unpublish">true - unpublish and draft, false - archive</param>
    private async Task SendNotificationOfApprovalAsync(Guid cabId, string cabName, User submitter, User approver,
        bool unpublish)
    {
        var personalisation = new Dictionary<string, dynamic?>
        {
            { "CABName", cabName },
            { "Unpublish", unpublish },
            { "Archive", !unpublish }
        };

        await _notificationClient.SendEmailAsync(submitter.EmailAddress,
            _templateOptions.NotificationUnpublishApproved, personalisation);

        await _notificationClient.SendEmailAsync(_templateOptions.UkasGroupEmail,
            _templateOptions.NotificationUnpublishApproved, personalisation);

        await _workflowTaskService.CreateAsync(
            new WorkflowTask(
                TaskType.RequestToUnpublishApproved,
                approver,
                // Approver becomes the submitter for Approved Notification
                submitter.RoleId,
                submitter,
                DateTime.Now,
                unpublish
                    ? $"The request to unpublish CAB {cabName} has been approved and it has been saved as a draft."
                    : $"The request to unpublish and archive CAB {cabName} has been approved.",
                approver,
                DateTime.Now,
                true,
                null,
                true,
                cabId));
    }
}