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
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers.Unarchive;

[Area("admin"), Route("admin/cab/unarchive/approve"), Authorize]
public class ApproveUnarchiveCABController : Controller
{
    private readonly ICABAdminService _cabAdminService;
    private readonly IUserService _userService;
    private readonly IAsyncNotificationClient _notificationClient;
    private readonly CoreEmailTemplateOptions _templateOptions;
    private readonly IWorkflowTaskService _workflowTaskService;
    private readonly TelemetryClient _telemetryClient;

    public ApproveUnarchiveCABController(
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
    public async Task<IActionResult> ApprovePostAsync(string cabUrl, ApproveUnarchiveCABViewModel vm)
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
        await Unarchive(cabUrl, currentUser, vm.IsPublish!.Value);

        return RedirectToRoute(CabManagementController.Routes.CABManagement);
    }

    /// <summary>
    /// Unarchive and save as draft then publish if publish is true
    /// </summary>
    /// <param name="cabUrl"></param>
    /// <param name="currentUser"></param>
    /// <param name="publish">publish flag</param>
    private async Task Unarchive(string cabUrl, UserAccount currentUser, bool publish)
    {
        var document = await GetArchivedDocumentAsync(cabUrl);
        var latestDocument = await _cabAdminService.UnarchiveDocumentAsync(currentUser, document.CABId, null, null);
        _telemetryClient.TrackEvent(AiTracking.Events.CabArchived, HttpContext.ToTrackingMetadata(new()
        {
            [AiTracking.Metadata.CabId] = document.CABId,
            [AiTracking.Metadata.CabName] = document.Name!
        }));
        if (publish)
        {
            await _cabAdminService.PublishDocumentAsync(currentUser, latestDocument);
        }
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