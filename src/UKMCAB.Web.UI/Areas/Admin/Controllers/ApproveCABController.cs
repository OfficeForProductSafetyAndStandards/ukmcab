using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Notify.Interfaces;
using UKMCAB.Common.Exceptions;
using UKMCAB.Common.Extensions;
using UKMCAB.Core.Domain.Workflow;
using UKMCAB.Core.EmailTemplateOptions;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Core.Services.Workflow;
using UKMCAB.Data;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Areas.Search.Controllers;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.PublishApproval;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers;

[Area("admin"), Authorize(Policy = Policies.ApproveRequests)]
public class ApproveCABController : Controller
{
    private readonly ICABAdminService _cabAdminService;
    private readonly IUserService _userService;
    private readonly IAsyncNotificationClient _notificationClient;
    private readonly CoreEmailTemplateOptions _templateOptions;
    private readonly IWorkflowTaskService _workflowTaskService;
    private readonly IEditLockService _editLockService;

    public ApproveCABController(
        ICABAdminService cabAdminService,
        IUserService userService,
        IAsyncNotificationClient notificationClient,
        IOptions<CoreEmailTemplateOptions> templateOptions,
        IWorkflowTaskService workflowTaskService,
        IEditLockService editLockService)
    {
        _cabAdminService = cabAdminService;
        _userService = userService;
        _notificationClient = notificationClient;
        _workflowTaskService = workflowTaskService;
        _editLockService = editLockService;
        _templateOptions = templateOptions.Value;
    }

    public static class Routes
    {
        public const string ApproveAndSetCabNumber = "cab.approvesetcabnumber";
        public const string Approve = "cab.approve";
    }

    [HttpGet("admin/cab/approve/{id}", Name = Routes.Approve)]
    public async Task<IActionResult> ApproveAsync(string id, string? returnUrl)
    {
        returnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : default;
        var cabId = Guid.Parse(id);
        var document = await GetDocumentAsync(cabId);
        if (document.StatusValue != Status.Draft || document.SubStatus != SubStatus.PendingApprovalToPublish)
        {
            throw new PermissionDeniedException("CAB status needs to be Submitted for approval");
        }

        if (string.IsNullOrEmpty(document.CABNumber) || string.IsNullOrEmpty(document.CabNumberVisibility))
        {
            return RedirectToRoute(Routes.ApproveAndSetCabNumber, new { id });
        } 

        var model = new UserNotesReasonViewModel(cabId, document.Name ?? throw new InvalidOperationException(), returnUrl, "Approve CAB");
        return View("~/Areas/Admin/Views/CAB/PublishApproval/Approve.cshtml", model);
    }

    [HttpPost("admin/cab/approve/{id}", Name = Routes.Approve)]
    public async Task<IActionResult> ApprovePostAsync(string id, UserNotesReasonViewModel vm)
    {
        ModelState.Remove(nameof(UserNotesReasonViewModel.CabName));
        if (!ModelState.IsValid)
        {
            vm.Title = "Approve CAB";
            return View("~/Areas/Admin/Views/CAB/PublishApproval/Approve.cshtml", vm);
        }

        var cabId = Guid.Parse(id);
        var document = await GetDocumentAsync(cabId);

        await ApproveAsync(document, vm.UserNotes, vm.Reason);
        await _editLockService.RemoveEditLockForCabAsync(document.CABId);
        return RedirectToRoute(CabManagementController.Routes.CABManagement);
    }

    [HttpGet("admin/cab/approve-setcabnumber/{id}", Name = Routes.ApproveAndSetCabNumber)]
    public async Task<IActionResult> ApproveSetCabNumberAsync(string id, string? returnUrl)
    {
        var cabId = Guid.Parse(id);
        var document = await GetDocumentAsync(cabId);
        returnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : default;
        if (document.StatusValue != Status.Draft || document.SubStatus != SubStatus.PendingApprovalToPublish)
        {
            throw new PermissionDeniedException("CAB status needs to be Submitted for approval");
        }

        if (string.IsNullOrEmpty(document.CABNumber) || string.IsNullOrEmpty(document.CabNumberVisibility))
        {
            var model = new ApproveCABViewModel(cabId, document.Name ?? throw new InvalidOperationException(), returnUrl, "Approve CAB");
            return View("~/Areas/Admin/Views/CAB/PublishApproval/ApproveSetCabNumber.cshtml", model);
        }
        else
        {
            throw new PermissionDeniedException("CAB number already set for this cab");
        }
    }

    [HttpPost("admin/cab/approve-setcabnumber/{id}", Name = Routes.ApproveAndSetCabNumber)]
    public async Task<IActionResult> ApproveSetCabNumberAsync(string id, ApproveCABViewModel vm)
    {
        var cabId = Guid.Parse(id);
        var document = await GetDocumentAsync(cabId);
        ModelState.Remove(nameof(ApproveCABViewModel.CabName));

        var duplicateDocuments =
                   await _cabAdminService.FindOtherDocumentsByCabNumberOrUkasReference(document.CABId,
                       vm.CABNumber, null);

        if (duplicateDocuments.Any())
        {
            if (vm.CABNumber != null && duplicateDocuments.Any(d => d.CABNumber.DoesEqual(vm.CABNumber)))
            {
                ModelState.AddModelError(nameof(vm.CABNumber), "This CAB number already exists\r\n\r\n");
            }
        }

        if (!ModelState.IsValid)
        {
            vm.Title = "Approve CAB";
            return View("~/Areas/Admin/Views/CAB/PublishApproval/ApproveSetCabNumber.cshtml", vm);
        }

        document.CABNumber = vm.CABNumber;
        document.PreviousCABNumbers = vm.PreviousCABNumbers;
        document.CabNumberVisibility = vm.CabNumberVisibility;

        await ApproveAsync(document, vm.UserNotes, vm.Reason);

        await _editLockService.RemoveEditLockForCabAsync(document.CABId);
        return RedirectToRoute(CabManagementController.Routes.CABManagement);
    }

    private async Task ApproveAsync(Document document, string? userNotes, string? reason)
    {
        var cabId = Guid.Parse(document.CABId);

        var user =
           await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value) ??
           throw new InvalidOperationException("User account not found");
        var userRoleId = Roles.List.First(r => r.Id == user.Role).Id;

        await _cabAdminService.RemoveLegislativeAreasToApprovedToRemoveByOPSS(document);

        var clonedDocument = document.DeepCopy();
        clonedDocument.SubStatus = SubStatus.None;

        var publishType = TempData["PublishType"] != null ? (string)TempData["PublishType"] : DataConstants.PublishType.MajorPublish;

        await _cabAdminService.PublishDocumentAsync(user, document, userNotes, reason, publishType);

        if (clonedDocument.CreatedByUserGroup == Roles.OPSS.Id && clonedDocument.DocumentLegislativeAreas.Any(la => la.Status == LAStatus.Draft))
        {
            clonedDocument.DocumentLegislativeAreas.ForEach(la => la.Status = LAStatus.Published);
        }
        else
        {
            clonedDocument.DocumentLegislativeAreas.Where(la => la.Status is 
                LAStatus.ApprovedByOpssAdmin or LAStatus.DeclinedToRemoveByOGD or LAStatus.DeclinedToRemoveByOPSS or
                LAStatus.DeclinedToArchiveAndArchiveScheduleByOGD or LAStatus.DeclinedToArchiveAndArchiveScheduleByOPSS or
                LAStatus.DeclinedToArchiveAndRemoveScheduleByOGD or LAStatus.DeclinedToArchiveAndRemoveScheduleByOPSS).ForEach(la => la.Status = LAStatus.Published);
        }

        if (clonedDocument.DocumentLegislativeAreas.Any(la => la.Status == LAStatus.PendingApproval || la.Status == LAStatus.Approved))
        {
            await _cabAdminService.CreateDocumentAsync(user, clonedDocument);
            await _cabAdminService.SetSubStatusAsync(Guid.Parse(clonedDocument.CABId), Status.Draft, SubStatus.PendingApprovalToPublish, new Audit(user, AuditCABActions.Created));
        }
        else if (clonedDocument.DocumentLegislativeAreas.Any(la => la.Status == LAStatus.Declined || la.Status == LAStatus.DeclinedByOpssAdmin))
        {
            await _cabAdminService.CreateDocumentAsync(user, clonedDocument);
        }        

        var submitTask = await MarkTaskAsCompleteAsync(cabId,
           new User(user.Id, user.FirstName, user.Surname, userRoleId,
               user.EmailAddress ?? throw new InvalidOperationException()));
        await SendNotificationOfApprovalAsync(cabId, document.Name ?? throw new InvalidOperationException(),
            submitTask.Submitter);      

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