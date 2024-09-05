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
using UKMCAB.Data.Models.Users;
using UKMCAB.Web.UI.Areas.Search.Controllers;
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
        var allDocuments = await _cabAdminService.FindAllDocumentsByCABIdAsync(cabId);
        var hasExistingVersion = allDocuments.Any(d => d.id.ToString() != document.id);

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

        var deleter = await _userService.GetAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)) ?? throw new InvalidOperationException();

        var auditLogs = document.AuditLog
            .Where(x => (new[] { AuditCABActions.Created, AuditCABActions.Unarchived }).Contains(x.Action))
            .OrderByDescending(x => x.DateTime);
        if(!auditLogs.Any()) throw new InvalidOperationException("CAB must have audit entry matching Created or Unarchived");

        var creator = await _userService.GetAsync(auditLogs.First().UserId) ?? throw new InvalidOperationException();
        await _cabAdminService.DeleteDraftDocumentAsync(deleter, cabId, vm.DeleteReason);
        await SendNotificationOfDeletionAsync(Guid.Parse(document.CABId), document.Name!, deleter, creator!, vm.DeleteReason);

        return RedirectToRoute(CabManagementController.Routes.CABManagement);
    }

    private void EnsureCabStatusIsDraft(Document document)
    {
        if (document.StatusValue != Status.Draft || document.SubStatus != SubStatus.None)
        {
            throw new InvalidOperationException("CAB status needs to be Draft and sub-status needs to be None for delete");
        }
    }

    /// <summary>
    /// Sends an email and notification for a deleted cab.
    /// </summary>
    /// <param name="cabName">Name of CAB</param>
    /// <param name="deleterAccount"></param>
    /// <param name="draftCreator"></param>
    /// <param name="deleteReason"></param>
    private async Task SendNotificationOfDeletionAsync(Guid cabId, string cabName, UserAccount deleterAccount, UserAccount draftCreator, string? deleteReason)
    {
        var deleterRoleId = Roles.RoleId(deleterAccount.Role) ?? throw new InvalidOperationException();

        var deleter = new User(deleterAccount.Id, deleterAccount.FirstName, deleterAccount.Surname,
            deleterRoleId, deleterAccount.EmailAddress ?? throw new InvalidOperationException());

        var cabCreatorRoleId = Roles.RoleId(draftCreator.Role) ?? throw new InvalidOperationException();

        var assignee = new User(draftCreator.Id, draftCreator.FirstName, draftCreator.Surname,
            cabCreatorRoleId, draftCreator.EmailAddress ?? throw new InvalidOperationException());

        var userGroup = Roles.NameFor(deleter.RoleId);
        var personalisation = new Dictionary<string, dynamic?>
        {
            { "CABName", cabName },
            { "UserGroup", userGroup },
            { "HasDeleteReason", !string.IsNullOrEmpty(deleteReason) },
            { "HasNoDeleteReason", string.IsNullOrEmpty(deleteReason) },
            { "DeleteReason", deleteReason ?? string.Empty },
            {
                "NotificationsURL", UriHelper.GetAbsoluteUriFromRequestAndPath(HttpContext.Request,
                    Url.RouteUrl(Admin.Controllers.NotificationController.Routes.Notifications))
            }
        };
        await _notificationClient.SendEmailAsync(draftCreator.EmailAddress,
            _templateOptions.NotificationDraftCabDeleted, personalisation);

        await _workflowTaskService.CreateAsync(
            new WorkflowTask(
                TaskType.DraftCabDeleted,
                deleter,
                assignee.RoleId,
                assignee,
                DateTime.Now,
                deleteReason ?? $"{userGroup} has deleted CAB {cabName}",
                deleter,
                DateTime.Now,
                false,
                null,
                true,
                deleteReason != null ? cabId : null));
    }
}