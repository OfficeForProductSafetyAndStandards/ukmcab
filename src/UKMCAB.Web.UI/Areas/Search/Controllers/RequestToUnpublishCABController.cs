using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Notify.Interfaces;
using UKMCAB.Core.Domain.Workflow;
using UKMCAB.Core.EmailTemplateOptions;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Core.Services.Workflow;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Models.ViewModels.Search.RequestToUnpublishCAB;

namespace UKMCAB.Web.UI.Areas.Search.Controllers;

[Area("search"), Route("search/request-to-unpublish/"), Authorize]
public class RequestToUnpublishCABController : Controller
{
    private readonly ICABAdminService _cabAdminService;
    private readonly IWorkflowTaskService _workflowTaskService;
    private readonly IUserService _userService;
    private readonly IAsyncNotificationClient _notificationClient;
    private readonly CoreEmailTemplateOptions _templateOptions;

    public static class Routes
    {
        public const string RequestUnpublish = "cab.request.unpublish";
        public const string RequestUnpublishConfirmation = "cab.request.unpublish.confirmation";
    }

    public RequestToUnpublishCABController(ICABAdminService cabAdminService, IWorkflowTaskService workflowTaskService,
        IUserService userService, IAsyncNotificationClient notificationClient,
        IOptions<CoreEmailTemplateOptions> options)
    {
        _cabAdminService = cabAdminService;
        _workflowTaskService = workflowTaskService;
        _userService = userService;
        _notificationClient = notificationClient;
        _templateOptions = options.Value;
    }

    [HttpGet("{cabUrl}", Name = Routes.RequestUnpublish)]
    public async Task<IActionResult> IndexAsync(string cabUrl)
    {
        var publishedDocument = await GetPublishedDocumentAsync(cabUrl);
        if (publishedDocument.SubStatus != SubStatus.None)
        {
            return RedirectToRoute(CABProfileController.Routes.CabDetails, new { id = publishedDocument.CABId });
        }

        var vm =
            new RequestToUnpublishCABViewModel(publishedDocument.Name ?? throw new InvalidOperationException(),
                publishedDocument.URLSlug, Guid.Parse(publishedDocument.CABId))
            {
                Title = "Request to Unpublish CAB"
            };
        return View(vm);
    }

    /// <summary>
    /// Get the Published document
    /// </summary>
    /// <param name="cabUrl">url slug to get</param>
    /// <returns>document with archived status</returns>
    private async Task<Document> GetPublishedDocumentAsync(string cabUrl)
    {
        var documents = await _cabAdminService.FindAllDocumentsByCABURLAsync(cabUrl);
        return documents.First(d => d.StatusValue == Status.Published);
    }

    [HttpPost("{cabUrl}", Name = Routes.RequestUnpublish)]
    public async Task<IActionResult> Index(RequestToUnpublishCABViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var currentUser = await _userService.GetAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)) ??
                          throw new InvalidOperationException();
        var userRoleId = Roles.List.First(r =>
            r.Label != null && r.Label.Equals(currentUser.Role, StringComparison.CurrentCultureIgnoreCase)).Id;
        var submitter = new User(currentUser.Id, currentUser.FirstName, currentUser.Surname,
            userRoleId ?? throw new InvalidOperationException(),
            currentUser.EmailAddress ?? throw new InvalidOperationException());
        
        await _cabAdminService.SetSubStatusAsync(vm.CabId, Status.Published,
            vm.IsUnpublish!.Value ? SubStatus.PendingApprovalToUnpublish : SubStatus.PendingApprovalToArchive,
            new Audit(currentUser, vm.IsUnpublish!.Value ? AuditCABActions.UnpublishApprovalRequest : AuditCABActions.ArchiveApprovalRequest));
        
        await _workflowTaskService.CreateAsync(new WorkflowTask(
            vm.IsUnpublish!.Value ? TaskType.RequestToUnpublish : TaskType.RequestToArchive,
            submitter,
            Roles.OPSS.Id,
            null,
            null,
            vm.Reason!,
            submitter,
            DateTime.Now,
            null,
            null,
            false,
            vm.CabId
        ));
        var personalisation = new Dictionary<string, dynamic?>
        {
            { "CABName", vm.CabName },
            {
                "CABUrl", UriHelper.GetAbsoluteUriFromRequestAndPath(HttpContext.Request,
                    Url.RouteUrl(CABProfileController.Routes.CabDetails, new { id = vm.CabId }))
            },
            { "UserGroup", Roles.NameFor(submitter.RoleId) },
            { "Name", submitter.FirstAndLastName },
            { "Unpublish", vm.IsUnpublish },
            { "Archive", !vm.IsUnpublish }, //No if else in Notify only if
            { "Reason", vm.Reason }
        };
        await _notificationClient.SendEmailAsync(_templateOptions.ApprovedBodiesEmail,
            _templateOptions.NotificationRequestToUnpublishCab, personalisation);
        return RedirectToRoute(Routes.RequestUnpublishConfirmation, new { cabUrl = vm.CabUrl });
    }

    [HttpGet("search/request-to-unpublish/confirmation/{cabUrl}", Name = Routes.RequestUnpublishConfirmation)]
    public async Task<IActionResult> CabRequestToUnpublishConfirmationAsync(string cabUrl)
    {
        var publishedDocument = await GetPublishedDocumentAsync(cabUrl);
        return View("Confirmation",new RequestToUnpublishConfirmationViewModel
        {
            Title = $"Request to unpublish CAB {publishedDocument.Name} has been submitted for approval",
            URLSlug = cabUrl
        });
    }
}