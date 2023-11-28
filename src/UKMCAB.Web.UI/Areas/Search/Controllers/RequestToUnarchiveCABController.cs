using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Notify.Interfaces;
using UKMCAB.Core.Domain.Workflow;
using UKMCAB.Core.EmailTemplateOptions;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Core.Services.Workflow;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Areas.Admin.Controllers;
using UKMCAB.Web.UI.Models.ViewModels.Search.RequestToUnarchiveCAB;

namespace UKMCAB.Web.UI.Areas.Search.Controllers;

[Area("search"), Route("search/request-to-unarchive/")]
public class RequestToUnarchiveCABController : Controller
{
    private readonly ICABAdminService _cabAdminService;
    private readonly IWorkflowTaskService _workflowTaskService;
    private readonly IUserService _userService;
    private readonly IAsyncNotificationClient _notificationClient;
    private readonly CoreEmailTemplateOptions _templateOptions;

    public static class Routes
    {
        public const string RequestUnarchive = "cab.request.unarchive";
    }

    public RequestToUnarchiveCABController(ICABAdminService cabAdminService, IWorkflowTaskService workflowTaskService,
        IUserService userService, IAsyncNotificationClient notificationClient, IOptions<CoreEmailTemplateOptions> options)
    {
        _cabAdminService = cabAdminService;
        _workflowTaskService = workflowTaskService;
        _userService = userService;
        _notificationClient = notificationClient;
        _templateOptions = options.Value;
    }

    [HttpGet("{cabUrl}", Name = Routes.RequestUnarchive)]
    public async Task<IActionResult> IndexAsync(string cabUrl)
    {
        var documents = await _cabAdminService.FindAllDocumentsByCABURLAsync(cabUrl);
        var archivedDocument = documents.First(d => d.StatusValue == Status.Archived);
        var vm =
            new RequestToUnarchiveCABViewModel(archivedDocument.Name ?? throw new InvalidOperationException(),
                archivedDocument.URLSlug, Guid.Parse(archivedDocument.CABId))
            {
                Title = "Request to unarchive CAB"
            };
        return View(vm);
    }

    [HttpPost("{cabUrl}", Name = Routes.RequestUnarchive)]
    public async Task<IActionResult> IndexAsync(RequestToUnarchiveCABViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var currentUser = await _userService.GetAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)) ??
                          throw new InvalidOperationException();
        var submitter = new User(currentUser.Id, currentUser.FirstName, currentUser.Surname,
            currentUser.Role ?? throw new InvalidOperationException(),
            currentUser.EmailAddress ?? throw new InvalidOperationException());
        await _workflowTaskService.CreateAsync(new WorkflowTask(
            Guid.NewGuid(),
            vm.IsPublish!.Value ? TaskType.RequestToUnarchiveForPublish: TaskType.RequestToUnarchiveForDraft,
            submitter,
            Roles.OPSS.Id,
            null,
            null,
            vm.Reason!,
            DateTime.Now,
            submitter,
            DateTime.Now,
            null,
            null,
            false,
            vm.CabId
        ));
        var personalisation = new Dictionary<string, dynamic?>
        {
            { "CABName", vm.CABName },
            { "UserGroup", Roles.NameFor(submitter.RoleId) },
            { "Name", submitter.FirstAndLastName },
            { "Published", vm.IsPublish },
            { "Draft", !vm.IsPublish }, //No if else in Notify only if
            { "Reason", vm.Reason }
        };
        await _notificationClient.SendEmailAsync(_templateOptions.ApprovedBodiesEmail,
            _templateOptions.NotificationUnarchiveForApproval, personalisation);
        return RedirectToRoute(CabManagementController.Routes.CABManagement);
    }
}