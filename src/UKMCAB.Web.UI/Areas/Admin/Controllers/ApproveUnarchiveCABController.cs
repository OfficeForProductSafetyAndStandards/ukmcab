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

[Area("admin"), Route("admin/cab/unarchive/approve"), Authorize]
public class ApproveUnarchiveCABController : Controller
{
    private readonly ICABAdminService _cabAdminService;
    private readonly IUserService _userService;
    private readonly IAsyncNotificationClient _notificationClient;
    private readonly CoreEmailTemplateOptions _templateOptions;
    private readonly IWorkflowTaskService _workflowTaskService;

    public ApproveUnarchiveCABController(
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
        public const string Approve = "cab.unarchive.approve";
    }

    [HttpGet("{cabUrl}", Name = Routes.Approve)]
    public async Task<IActionResult> ApproveAsync(string cabUrl)
    {
        var document = await GetArchivedDocumentAsync(cabUrl);
        if (document.StatusValue != Status.Archived || document.SubStatus != SubStatus.PendingApproval)
        {
            throw new PermissionDeniedException("CAB status needs to be Submitted for approval");
        }
        
        var tasks = await _workflowTaskService.GetByCabIdAsync(Guid.Parse(document.CABId));
        var task = tasks.First(t => t.TaskType is TaskType.RequestToUnarchiveForDraft or TaskType.RequestToUnarchiveForPublish && !t.Completed);
        var vm = new ApproveUnarchiveCABViewModel(
            "Approve unarchive CAB",
            document.Name ?? throw new InvalidOperationException(), 
            document.URLSlug,
            Guid.Parse(document.CABId), 
            task.Submitter.UserGroup, 
            task.Submitter.FirstAndLastName,
            task.TaskType == TaskType.RequestToUnarchiveForPublish);

        return View("~/Areas/Admin/Views/CAB/Unarchive/Approve.cshtml", vm);
    }

    [HttpPost("{cabUrl}", Name = Routes.Approve)]
    public async Task<IActionResult> ApprovePostAsync(ApproveUnarchiveCABViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Areas/Admin/Views/CAB/Unarchive/Approve.cshtml", vm);
        }

        var currentUser = await _userService.GetAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)) ??
                          throw new InvalidOperationException();
        var userRoleId = Roles.List.First(r =>
            r.Label != null && r.Label.Equals(currentUser.Role, StringComparison.CurrentCultureIgnoreCase)).Id;
        var submitter = new User(currentUser.Id, currentUser.FirstName, currentUser.Surname,
            userRoleId ?? throw new InvalidOperationException(),
            currentUser.EmailAddress ?? throw new InvalidOperationException());
        
        return RedirectToRoute(CabManagementController.Routes.CABManagement);
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
}