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
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Unarchive;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers.Unarchive;

[Area("admin"), Route("admin/cab/unarchive/decline"), Authorize(Policies.ApproveRequests)]
public class DeclineUnarchiveCABController : Controller
{
    private readonly ICABAdminService _cabAdminService;
    private readonly IUserService _userService;
    private readonly IAsyncNotificationClient _notificationClient;
    private readonly CoreEmailTemplateOptions _templateOptions;
    private readonly IWorkflowTaskService _workflowTaskService;

    public DeclineUnarchiveCABController(
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
        public const string Decline = "cab.unarchive.decline";
    }

    [HttpGet("{cabUrl}", Name = Routes.Decline)]
    public async Task<IActionResult> DeclineAsync(string cabUrl)
    {
        var document = await GetArchivedDocumentAsync(cabUrl);
        var unarchiveStatuses = new List<SubStatus>()
        {
            SubStatus.PendingApprovalToUnarchive,
            SubStatus.PendingApprovalToUnarchivePublish
        };
        if (document.StatusValue != Status.Archived || !unarchiveStatuses.Contains(document.SubStatus))
        {
            throw new PermissionDeniedException("CAB status needs to be Submitted for approval");
        }

        var task = await GetWorkflowTaskAsync(document.CABId);
        var vm = new DeclineUnarchiveCABViewModel(
            "Decline unarchive CAB",
            document.Name ?? throw new InvalidOperationException(),
            document.URLSlug,
            Guid.Parse(document.CABId),
            task.Submitter.UserGroup);

        return View("~/Areas/Admin/Views/CAB/Unarchive/Decline.cshtml", vm);
    }

    [HttpPost("{cabUrl}", Name = Routes.Decline)]
    public async Task<IActionResult> DeclinePostAsync(string cabUrl, DeclineUnarchiveCABViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Areas/Admin/Views/CAB/Unarchive/Decline.cshtml", vm);
        }

        var currentUser = await _userService.GetAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)) ??
                          throw new InvalidOperationException();
        var userRoleId = Roles.List.First(r =>
            r.Label != null && r.Label.Equals(currentUser.Role, StringComparison.CurrentCultureIgnoreCase)).Id;
        if (userRoleId != Roles.OPSS.Id)
        {
            throw new PermissionDeniedException("User Permission denied to decline an unarchive request");
        }
        var approver = new User(currentUser.Id, currentUser.FirstName, currentUser.Surname,
            userRoleId ?? throw new InvalidOperationException(),
            currentUser.EmailAddress ?? throw new InvalidOperationException());
        var requestTask = await MarkTaskAsCompleteAsync(vm.CabId, approver);
        await SendNotificationOfDeclineAsync(vm.CabId, vm.CABName, requestTask.Submitter, approver, vm.Reason!);
        return RedirectToRoute(CabManagementController.Routes.CABManagement);
    }
    
    private async Task<WorkflowTask> GetWorkflowTaskAsync(string cabId)
    {
        var tasks = await _workflowTaskService.GetByCabIdAsync(Guid.Parse(cabId));
        var task = tasks.First(t =>
            t.TaskType is TaskType.RequestToUnarchiveForDraft or TaskType.RequestToUnarchiveForPublish && !t.Completed);
        return task;
    }

    /// <summary>
    /// Get the Archived document
    /// </summary>
    /// <param name="cabUrl">url slug to get</param>
    /// <returns>document with archived status</returns>
    private async Task<Document> GetArchivedDocumentAsync(string cabUrl)
    {
        var documents = await _cabAdminService.FindAllDocumentsByCABURLAsync(cabUrl);
        var archivedDocument = documents.First(d => d.StatusValue == Status.Archived);
        return archivedDocument;
    }

    /// <summary>
    /// Mark incoming Request to unarchive task as completed
    /// </summary>
    /// <param name="cabId">Associated CAB</param>
    /// <param name="userLastUpdatedBy"></param>
    private async Task<WorkflowTask> MarkTaskAsCompleteAsync(Guid cabId, User userLastUpdatedBy)
    {
        var task = await GetWorkflowTaskAsync(cabId.ToString());
        await _workflowTaskService.MarkTaskAsCompletedAsync(task.Id, userLastUpdatedBy);
        return task;
    }

    /// <summary>
    /// Sends an email and notification for declined unarchive
    /// </summary>
    /// <param name="cabId">CAB id</param>
    /// <param name="cabName">Name of CAB</param>
    /// <param name="submitter"></param>
    /// <param name="decliner"></param>
    /// <param name="reason"></param>
    private async Task SendNotificationOfDeclineAsync(Guid cabId, string cabName, User submitter, User decliner, string reason)
    {
        var personalisation = new Dictionary<string, dynamic?>
        {
            { "CABName", cabName },
            {
                "CABUrl",
                UriHelper.GetAbsoluteUriFromRequestAndPath(HttpContext.Request,
                    Url.RouteUrl(CABProfileController.Routes.CabDetails, new { id = cabId }))
            },
            { "Reason", reason }
        };
        await _notificationClient.SendEmailAsync(submitter.EmailAddress,
            _templateOptions.NotificationUnarchiveDeclined, personalisation);

        await _notificationClient.SendEmailAsync(_templateOptions.UkasGroupEmail,
           _templateOptions.NotificationUnarchiveDeclined, personalisation);

        await _workflowTaskService.CreateAsync(
            new WorkflowTask(
               TaskType.RequestToUnarchiveDeclined,
                decliner,
                // Approver becomes the submitter for Approved Notification
                submitter.RoleId,
                submitter,
                DateTime.Now,
                $"The request to unarchive CAB {cabName} has been declined.",
                decliner,
                DateTime.Now,
                false,
                reason,
                true,
                cabId));
    }
}