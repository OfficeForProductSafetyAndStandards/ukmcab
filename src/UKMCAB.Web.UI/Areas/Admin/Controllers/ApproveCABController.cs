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

[Area("admin"), Authorize]
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

    [HttpGet("/cab/approve/{id}", Name = Routes.Approve)]
    public async Task<IActionResult> Approve(string id)
    {
        var document = await _cabAdminService.GetLatestDocumentAsync(id) ??
                       throw new InvalidOperationException("CAB not found");
        if (document.StatusValue != Status.Draft || document.SubStatus != SubStatus.PendingApproval)
        {
            throw new PermissionDeniedException("CAB status needs to be Submitted for approval");
        }

        var model = new ApproveCABViewModel("Approve CAB", document.CABId,
            document.Name ?? throw new InvalidOperationException(), string.Empty);

        return View("~/Areas/Admin/Views/CAB/Approve.cshtml", model);
    }

    [HttpPost("/cab/approve/{id}")]
    public async Task<IActionResult> ApprovePost(string cabId, string cabNumber)
    {
        var document = await _cabAdminService.GetLatestDocumentAsync(cabId) ??
                       throw new InvalidOperationException("CAB not found");

        var model = new ApproveCABViewModel("Approve CAB", document.CABId,
            document.Name ?? throw new InvalidOperationException(), cabNumber);
        if (!ModelState.IsValid)
        {
            return View("~/Areas/Admin/Views/CAB/Approve.cshtml", model);
        }

        document.CABNumber = model.CABNumber;
        await _cabAdminService.PublishDocumentAsync(
            await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value) ??
            throw new InvalidOperationException("User account not found"), document);

        return View("~/Areas/Admin/Views/CAB/Approve.cshtml", model);
    }

    /// <summary>
    /// Sends an email and notification for approved cab
    /// </summary>
    /// <param name="cabId">CAB id</param>
    /// <param name="cabName">Name of CAB</param>
    private async Task SendNotificationOfApproval(Guid cabId, string cabName)
    {
        var personalisation = new Dictionary<string, dynamic?>
        {
            { "CABName", cabName },
            {
                "CABUrl",
                UriHelper.GetAbsoluteUriFromRequestAndPath(HttpContext.Request,
                    Url.RouteUrl(CABProfileController.Routes.CabDetails, new {id = cabId}))
            }
        };
        
        var tasks = await _workflowTaskService.GetByCabIdAsync(cabId);
        var task = tasks.First(t => t.TaskType == TaskType.RequestToPublish);
        await _notificationClient.SendEmailAsync(task.Submitter.emailAddress,
            _templateOptions.NotificationCabApprovedEmail, personalisation);

       
        // if (publishModel.CabDetailsViewModel != null)
        // {
        //     await _workflowTaskService.CreateAsync(new WorkflowTask(Guid.NewGuid(), TaskType.RequestToPublish,
        //         new User(userAccount.Id, userAccount.FirstName, userAccount.Surname, userRoleId),
        //         Roles.OPSS.Id, null, null,
        //         $"{userAccount.FirstName} {userAccount.Surname} from {Roles.NameFor(userRoleId)} has submitted a request to approve and publish {publishModel.CabDetailsViewModel.Name}.",
        //         DateTime.Now,
        //         new User(userAccount.Id, userAccount.FirstName, userAccount.Surname, userRoleId), DateTime.Now,
        //         null, null,
        //         false, Guid.Parse(publishModel.CABId ?? throw new InvalidOperationException())));
        // }
    }
}