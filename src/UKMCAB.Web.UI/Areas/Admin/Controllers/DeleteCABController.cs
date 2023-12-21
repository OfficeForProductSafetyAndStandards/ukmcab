using System.Security.Claims;
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
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers;

[Area("admin"), Route("admin/cab/delete"), Authorize]
public class DeleteCABController : Controller
{
    private readonly ICABAdminService _cabAdminService;
    private readonly IUserService _userService;
    private readonly IAsyncNotificationClient _notificationClient;
    private readonly CoreEmailTemplateOptions _templateOptions;
    private readonly IWorkflowTaskService _workflowTaskService;

    public DeleteCABController(
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
        public const string Delete = "cab.delete";
    }

    [HttpGet("{cabId}", Name = Routes.Delete)]
    public async Task<IActionResult> DeleteAsync(string cabId)
    {
        var document = await _cabAdminService.GetLatestDocumentAsync(cabId) ??
                       throw new InvalidOperationException("CAB not found");

        EnsureCabStatusIsDraft(document);

        // Check if there is an existing published version. If found, user has to enter a delete reason which is stored against it.
        var allDocuments = await _cabAdminService.FindDocumentsByCABIdAsync(cabId);
        var hasExistingVersion = allDocuments.Any(d => d.Id.ToString() != document.id);

        var model = new DeleteCABViewModel(
            "Delete draft CAB profile",
            document.CABId ?? throw new InvalidOperationException(),
            document.Name ?? throw new InvalidOperationException(),
            hasExistingVersion
        );

        return View("~/Areas/Admin/Views/CAB/Delete.cshtml", model);
    }

    [HttpPost("{cabId}")]
    public async Task<IActionResult> DeletePostAsync(Guid cabId, DeleteCABViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Areas/Admin/Views/CAB/Delete.cshtml", vm);
        }

        var document = await _cabAdminService.GetLatestDocumentAsync(cabId.ToString()) ??
                       throw new InvalidOperationException("CAB not found");

        EnsureCabStatusIsDraft(document);

        var user = await _userService.GetAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)) ?? throw new InvalidOperationException();
        var userRoleId = Roles.List.First(r => r.Label != null && r.Label.Equals(user.Role, StringComparison.CurrentCultureIgnoreCase)).Id;

        await _cabAdminService.DeleteDraftDocumentAsync(user, cabId, vm.DeleteReason);

        // TODO:

        //var submitTask = await MarkTaskAsCompleteAsync(cabId,
        //    new User(user.Id, user.FirstName, user.Surname, userRoleId,
        //        user.EmailAddress ?? throw new InvalidOperationException()));

        //await SendNotificationOfDeclineAsync(cabId, document.Name, submitTask.Submitter, vm.DeleteReason);

        return RedirectToRoute(CabManagementController.Routes.CABManagement);
    }

    private void EnsureCabStatusIsDraft(Document document)
    {
        if (document.StatusValue != Status.Draft || document.SubStatus != SubStatus.None)
        {
            throw new InvalidOperationException("CAB status needs to be Draft and sub-status needs to be None for delete");
        }
    }

    ///// <summary>
    ///// Mark incoming Request to publish task as completed
    ///// </summary>
    ///// <param name="cabId">Associated CAB</param>
    ///// <param name="userLastUpdatedBy"></param>
    //private async Task<WorkflowTask> MarkTaskAsCompleteAsync(Guid cabId, User userLastUpdatedBy)
    //{
    //    var tasks = await _workflowTaskService.GetByCabIdAsync(cabId);
    //    var task = tasks.First(t => t is { TaskType: TaskType.RequestToPublish, Completed: false });
    //    await _workflowTaskService.MarkTaskAsCompletedAsync(task.Id, userLastUpdatedBy);
    //    return task;
    //}

    ///// <summary>
    ///// Sends an email and notification for declined cab
    ///// </summary>
    ///// <param name="cabId">CAB id</param>
    ///// <param name="cabName">Name of CAB</param>
    ///// <param name="submitter"></param>
    ///// <param name="declineReason"></param>
    //private async Task SendNotificationOfDeclineAsync(Guid cabId, string? cabName, User submitter, string declineReason)
    //{
    //    if (cabName == null) throw new ArgumentNullException(nameof(cabName));
    //    var personalisation = new Dictionary<string, dynamic?>
    //    {
    //        { "CABName", cabName },
    //        {
    //            "CABUrl",
    //            UriHelper.GetAbsoluteUriFromRequestAndPath(HttpContext.Request,
    //                Url.RouteUrl(CABController.Routes.CabSummary, new { id = cabId }))
    //        },
    //        { "DeclineReason", declineReason }
    //    };
    //    await _notificationClient.SendEmailAsync(submitter.EmailAddress,
    //        _templateOptions.NotificationCabDeclined, personalisation);
    //    var user =
    //        await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value) ??
    //        throw new InvalidOperationException();
    //    var approver = new User(user.Id, user.FirstName, user.Surname,
    //        user.Role ?? throw new InvalidOperationException(),
    //        user.EmailAddress ?? throw new InvalidOperationException());
    //    await _workflowTaskService.CreateAsync(
    //        new WorkflowTask(Guid.NewGuid(),
    //            TaskType.CABDeclined,
    //            approver,
    //            // Approver becomes the submitter for Declined CAB Notification
    //            submitter.RoleId,
    //            submitter,
    //            DateTime.Now,
    //            $"The request to approve CAB {cabName} has been declined for the following reason: {declineReason}.",
    //            DateTime.Now,
    //            approver,
    //            DateTime.Now,
    //            false,
    //            declineReason,
    //            true,
    //            cabId));
    //}
}