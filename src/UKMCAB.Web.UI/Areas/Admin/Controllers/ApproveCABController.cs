using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Notify.Interfaces;
using UKMCAB.Common.Exceptions;
using UKMCAB.Core.Domain;
using UKMCAB.Core.Domain.Workflow;
using UKMCAB.Core.EmailTemplateOptions;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Core.Services.Workflow;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Areas.Search.Controllers;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers;

[Area("admin"), Route("admin/cab/approve"), Authorize]
public class ApproveCABController : Controller
{
    private readonly ICABAdminService _cabAdminService;
    private readonly IUserService _userService;
    private readonly IAsyncNotificationClient _notificationClient;
    private readonly CoreEmailTemplateOptions _templateOptions;
    private readonly IWorkflowTaskService _workflowTaskService;

    public ApproveCABController(
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
        public const string Approve = "cab.approve";
    }

    [HttpGet("{cabId}", Name = Routes.Approve)]
    public async Task<IActionResult> ApproveAsync(Guid cabId)
    {
        var document = await GetDocumentAsync(cabId);
        if (document.StatusValue != Status.Draft || document.SubStatus != SubStatus.PendingApprovalToPublish)
        {
            throw new PermissionDeniedException("CAB status needs to be Submitted for approval");
        }

        var model = new ApproveCABViewModel("Approve CAB",
             document.Name ?? throw new InvalidOperationException());

        return View("~/Areas/Admin/Views/CAB/Approve.cshtml", model);
    }

    [HttpPost("{cabId}", Name = Routes.Approve)]
    public async Task<IActionResult> ApprovePostAsync(Guid cabId,
        [Bind(nameof(ApproveCABViewModel.CABNumber), nameof(CabNumberVisibility))] ApproveCABViewModel vm)
    {
        var document = await GetDocumentAsync(cabId);
        ModelState.Remove(nameof(ApproveCABViewModel.CabName));

        if (!ModelState.IsValid)
        {
            vm.Title = "Approve CAB";
            vm.CabName = document.Name??string.Empty;

            return View("~/Areas/Admin/Views/CAB/Approve.cshtml", vm);
        }

        document.CABNumber = vm.CABNumber;
        document.CabNumberVisibility  = vm.CabNumberVisibility;
        var user =
            await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value) ??
            throw new InvalidOperationException("User account not found");
        var userRoleId = Roles.List.First(r =>
            r.Label != null && r.Label.Equals(user.Role, StringComparison.CurrentCultureIgnoreCase)).Id;
        await _cabAdminService.PublishDocumentAsync(user, document);
        var submitTask = await MarkTaskAsCompleteAsync(cabId,
            new User(user.Id, user.FirstName, user.Surname, userRoleId,
                user.EmailAddress ?? throw new InvalidOperationException()));
        await SendNotificationOfApprovalAsync(cabId, document.Name ?? throw new InvalidOperationException(),
            submitTask.Submitter);
        return RedirectToRoute(CabManagementController.Routes.CABManagement);
    }

    private async Task<Document> GetDocumentAsync(Guid cabId)
    {
        var document = await _cabAdminService.GetLatestDocumentAsync(cabId.ToString()) ??
                       throw new InvalidOperationException("CAB not found");
        return document;
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
    /// Sends an email and notification for approved cab
    /// </summary>
    /// <param name="cabId">CAB id</param>
    /// <param name="cabName">Name of CAB</param>
    /// <param name="submitter"></param>
    private async Task SendNotificationOfApprovalAsync(Guid cabId, string cabName, User submitter)
    {
        var personalisation = new Dictionary<string, dynamic?>
        {
            { "CABName", cabName },
            {
                "CABUrl",
                UriHelper.GetAbsoluteUriFromRequestAndPath(HttpContext.Request,
                    Url.RouteUrl(CABProfileController.Routes.CabDetails, new { id = cabId }))
            }
        };
        await _notificationClient.SendEmailAsync(submitter.EmailAddress,
            _templateOptions.NotificationCabApproved, personalisation);

        // send email to submitter group email 
        await _notificationClient.SendEmailAsync(_templateOptions.UkasGroupEmail,
            _templateOptions.NotificationCabApproved, personalisation);

        var user =
            await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value) ??
            throw new InvalidOperationException();
        var approver = new User(user.Id, user.FirstName, user.Surname,
            user.Role ?? throw new InvalidOperationException(),
            user.EmailAddress ?? throw new InvalidOperationException());
        await _workflowTaskService.CreateAsync(
            new WorkflowTask(
                TaskType.CABPublished,
                approver,
                // Approver becomes the submitter for Approved CAB Notification
                submitter.RoleId,
                submitter,
                DateTime.Now,
                $"The request to publish CAB {cabName} has been approved.",
                approver,
                DateTime.Now,
                true,
                null,
                true,
                cabId));
    }
}