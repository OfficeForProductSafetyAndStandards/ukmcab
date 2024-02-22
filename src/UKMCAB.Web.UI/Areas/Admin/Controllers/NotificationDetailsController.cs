using Microsoft.AspNetCore.Authorization;
using UKMCAB.Common.Exceptions;
using UKMCAB.Common.Extensions;
using UKMCAB.Core.Domain.Workflow;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Core.Services.Workflow;
using UKMCAB.Data.Domain;
using UKMCAB.Web.UI.Areas.Search.Controllers;
using UKMCAB.Web.UI.Models.ViewModels.Admin.Notification;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers;

[Area("admin"), Route("admin/notifications"), Authorize]
public class NotificationDetailsController : Controller
{
    public static class Routes
    {
        public const string NotificationDetails = "admin.notification.details";
    }

    private readonly ICABAdminService _cabAdminService;
    private readonly IUserService _userService;
    private readonly IWorkflowTaskService _workflowTaskService;

    public NotificationDetailsController(ICABAdminService cabAdminService,
        IWorkflowTaskService workflowTaskService,
        IUserService userService
    )
    {
        _cabAdminService = cabAdminService;
        _workflowTaskService = workflowTaskService;
        _userService = userService;
    }


    [HttpGet("details/{id}", Name = Routes.NotificationDetails)]
    public async Task<IActionResult> Detail(Guid id)
    {
        var (notificationDetail, _) = await NotificationDetailsMapping(id);
        if (notificationDetail.IsSameUserGroup)
        {
            return View(notificationDetail);
        }
        throw new PermissionDeniedException();
    }


    [HttpPost("details/{id}", Name = Routes.NotificationDetails)]
    public async Task<IActionResult> Detail(Guid id, NotificationDetailViewModel model)
    {
        var (notificationDetail, workFlowTask) = await NotificationDetailsMapping(id);
        model.Status = notificationDetail.Status;
        model.From = notificationDetail.From;
        model.Subject = notificationDetail.Subject;
        model.Reason = notificationDetail.Reason;
        model.SentOn = notificationDetail.SentOn;
        model.CompletedOn = notificationDetail.CompletedOn;
        model.LastUpdated = notificationDetail.LastUpdated;
        model.ViewLink = notificationDetail.ViewLink;
        model.CompletedBy = notificationDetail.CompletedBy;
        model.AssignedOn = notificationDetail.AssignedOn;
        model.SelectAssignee = notificationDetail.SelectAssignee;
        model.SelectedAssignee = model.SelectedAssignee;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userAccount = await _userService.GetAsync(model.SelectedAssignee) ?? throw new InvalidOperationException();
        workFlowTask.Assignee = new User(model.SelectedAssignee, userAccount.FirstName, userAccount.Surname, userAccount.Role, userAccount.EmailAddress ?? throw new InvalidOperationException());
        workFlowTask.Assigned = DateTime.Now;
        await _workflowTaskService.UpdateAsync(workFlowTask);

        return RedirectToAction("Index", "Notification", new { Area = "admin" });
    }

    private async Task<(List<(string, string)>, string)> GetUser()
    {
        var options =
            new UserAccountListOptions(SkipTake.FromPage(-1, 500), new SortBy("firstName", null));
        var role = User.IsInRole(Roles.OPSS.Id) ? Roles.OPSS.Id : Roles.UKAS.Id;
        var users = await _userService.ListAsync(options);
        var assignees = users.Where(x => x.Role == role)
            .Select(user => new ValueTuple<string, string>(user.Id, user.FirstName! + " " + user.Surname)).ToList();
        return (assignees, role);
    }


    private async Task<(NotificationDetailViewModel, WorkflowTask)> NotificationDetailsMapping(Guid id)
    {
        var workFlowTask = await _workflowTaskService.GetAsync(id);
        var (assigneeList, group) = await GetUser();
        var notificationDetail = new NotificationDetailViewModel()
        {
            Status = workFlowTask.Completed ? "Completed" :
                workFlowTask.Assignee == null ? "Unassigned" : "Assigned",
            IsCompleted = workFlowTask.Completed,
            IsAssigned = workFlowTask.Assignee != null,
            From = workFlowTask.Submitter.FirstAndLastName,
            Subject = workFlowTask.TaskType.GetEnumDescription(),
            Reason = workFlowTask.Body,
            SentOn = workFlowTask.SentOn.ToStringBeisFormat(),
            CompletedOn = workFlowTask.Completed ? workFlowTask.LastUpdatedOn.ToStringBeisFormat() : string.Empty,
            LastUpdated = workFlowTask.LastUpdatedOn.ToStringBeisFormat(),
            CompletedBy = workFlowTask.LastUpdatedBy.FirstAndLastName,
            AssignedOn =
                workFlowTask.Assigned != null ? workFlowTask.Assigned.Value.ToStringBeisFormat() : string.Empty,
            SelectAssignee = assigneeList,
            UserGroup = Roles.NameFor(group),
            IsSameUserGroup = group.ToLowerInvariant().Trim().Equals(workFlowTask.ForRoleId.ToLowerInvariant().Trim()),
            SelectedAssignee = workFlowTask.Assignee?.FirstAndLastName!,
            SelectedAssigneeId = workFlowTask.Assignee?.UserId,
        };
        if (workFlowTask.Completed)
        {
            notificationDetail.UserGroup = Roles.NameFor(workFlowTask.LastUpdatedBy.RoleId);
        }
        if (workFlowTask.CABId == null) return (notificationDetail, workFlowTask);

        var cabs = await _cabAdminService.FindAllDocumentsByCABIdAsync(workFlowTask.CABId.ToString()!);
        var cabDetails = cabs.First();
        notificationDetail.ViewLink = workFlowTask.TaskType switch
        {
            TaskType.RequestToUnarchiveForDraft or TaskType.RequestToUnarchiveForPublish
                or TaskType.RequestToUnarchiveDeclined or TaskType.CABPublished or TaskType.RequestToUnpublish
                or TaskType.RequestToUnpublishDeclined or TaskType.RequestToArchive =>
                (cabDetails.Name,
                    Url.RouteUrl(CABProfileController.Routes.CabDetails, new { id = workFlowTask.CABId })),
            _ =>
                (cabDetails.Name,
                    Url.RouteUrl(CABController.Routes.CabSummary, new { id = workFlowTask.CABId })),
        };

        return (notificationDetail, workFlowTask);
    }
}