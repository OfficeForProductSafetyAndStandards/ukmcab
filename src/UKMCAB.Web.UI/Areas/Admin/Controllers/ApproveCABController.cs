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
        if (document.StatusValue != Status.Draft || document.SubStatus != SubStatus.PendingApproval)
        {
            throw new PermissionDeniedException("CAB status needs to be Submitted for approval");
        }

        var model = new ApproveCABViewModel("Approve CAB",
            document.Name ?? throw new InvalidOperationException());

        return View("~/Areas/Admin/Views/CAB/Approve.cshtml", model);
    }

    [HttpPost("{cabId}")]
    public async Task<IActionResult> ApprovePostAsync(Guid cabId, [Bind(nameof(ApproveCABViewModel.CABNumber))] ApproveCABViewModel vm)
    {
       var document = await GetDocumentAsync(cabId);
        ModelState.Remove(nameof(ApproveCABViewModel.CABName));
        if (!ModelState.IsValid)
        {
            vm.Title = "Approve CAB";
            return View("~/Areas/Admin/Views/CAB/Approve.cshtml", vm);
        }

        document.CABNumber = vm.CABNumber;
        await _cabAdminService.PublishDocumentAsync(
            await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value) ??
            throw new InvalidOperationException("User account not found"), document);
        var submitTask = await MarkTaskAsCompleteAsync(cabId);
        await SendNotificationOfApprovalAsync(cabId, document.Name, submitTask.Submitter);
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
    private async Task<WorkflowTask> MarkTaskAsCompleteAsync(Guid cabId)
    {
        var tasks = await _workflowTaskService.GetByCabIdAsync(cabId);
        var task = tasks.First(t => t.TaskType == TaskType.RequestToPublish);
        task.Completed = true;
        return await _workflowTaskService.UpdateAsync(task);
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
        var user =
            await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value) ?? throw new InvalidOperationException();
        var approver = new User(user.Id, user.FirstName, user.Surname, user.RoleId ?? throw new InvalidOperationException(),
            user.EmailAddress ?? throw new InvalidOperationException());
        await _workflowTaskService.CreateAsync(
            new WorkflowTask(Guid.NewGuid(), 
            TaskType.CABPublished,
            approver, 
            // Approver becomes the submitter for Approved CAB Notification
            submitter.RoleId, 
            submitter,
            DateTime.Now, 
            $"The request to publish CAB {cabName} has been approved.",
            DateTime.Now,
            approver, 
            DateTime.Now,
            true, 
            null,
            true, 
            cabId));
    }
}