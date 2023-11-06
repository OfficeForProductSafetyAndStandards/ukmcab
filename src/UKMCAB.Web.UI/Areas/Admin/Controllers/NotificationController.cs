using System.Globalization;
using UKMCAB.Core.Services.Users;
using Microsoft.AspNetCore.Authorization;
using UKMCAB.Core.Domain.Workflow;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Workflow;
using UKMCAB.Data.Domain;
using UKMCAB.Web.UI.Areas.Search.Controllers;
using UKMCAB.Web.UI.Models.ViewModels.Admin.Notification;
using UKMCAB.Web.UI.Models.ViewModels.Shared;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers;

[Area("admin"), Route("admin/notifications"), Authorize]
public class NotificationController : Controller
{
    public static class Routes
    {
        public const string NotificationsHome = "admin.notifications";
        public const string NotificationsAssignedToMe = "admin.notifications.assigned.me";
        public const string NotificationsAssignedToMyGroup = "admin.notifications.assigned.group";
        public const string NotificationsCompleted = "admin.notifications.completed";
        public const string NotificationDetails = "admin.notification.details";
    }

    private readonly IWorkflowTaskService _workflowTaskService;
    private readonly ICABAdminService _cabAdminService;
    private readonly IUserService _userService;

    public NotificationController(IWorkflowTaskService workflowTaskService,
        ICABAdminService cabAdminService,
        IUserService userService
    )
    {
        _workflowTaskService = workflowTaskService;
        _cabAdminService = cabAdminService;
        _userService = userService;
    }


    [HttpGet(Name = Routes.NotificationsHome)]
    public async Task<IActionResult> Index(string sf, string sd, int pageNumber = 1)
    {
        var role = User.IsInRole(Roles.OPSS.Id) ? Roles.OPSS.Id : Roles.UKAS.Id;
        var unassignedNotifications = await _workflowTaskService.GetUnassignedBySubmittedUserRoleAsync(role);
        List<(string From, string Subject, string CABName, string SentOn, string? CABLink)> items = new();
        foreach (var notification in unassignedNotifications)
        {
            var cab = await _cabAdminService.GetLatestDocumentAsync(notification.CABId.ToString());
            var item = (From: notification.Submitter.FirstAndLastName, Subject: notification.TaskType.ToString(),
                CABName: cab.Name, SentOn: notification.SentOn.ToString(CultureInfo.CurrentCulture),
                CABLink: Url.RouteUrl(Routes.NotificationDetails, notification.Id));
            items.Add(item);
        }

        var model = new NotificationsViewModel
        (
            Constants.PageTitle.Notifications,
            unassignedNotifications.Any(),
            sf,
            SortDirectionHelper.Get(sd),
            items,
            new PaginationViewModel
            {
                PageNumber = pageNumber,
                ResultsPerPage = 5,
                Total = unassignedNotifications.Count
            },
            new MobileSortTableViewModel(
                "asc",
                "sf",
                new List<Tuple<string, string>>()
                {
                    new("From", "From")
                }));
        return View(model);
    }

    [HttpGet("details/{id}", Name = Routes.NotificationDetails)]
    public async Task<IActionResult> Detail(string id)
    {
        var workFlowTask = await _workflowTaskService.GetAsync(Guid.Parse(id));
        var vm = new NotificationDetailViewModel()
        {
            NotificationTitle = "Notification Details", //TODO : check with BA
            Status = WorkFlowTaskStatus(workFlowTask.Completed, workFlowTask.Assignee),
            From = workFlowTask.Submitter.FirstName + " " +
                   workFlowTask.Submitter.Surname, //TODO : take latest property
            Subject = workFlowTask.TaskType.ToString(), //TODO : Bring the enum description rather than enum
            Reason = workFlowTask.Reason,
            SentOn = workFlowTask.SentOn
                .ToShortDateString(), //TODO: create helper for UK  DD/MM/YYYY 14:15 -- verify the profile page -- take Constants
            CompletedOn = "todo", // "commpleted on not found", //TODO : check with date -- need to take the latest code
            LastUpdated = workFlowTask.LastUpdatedOn.ToShortDateString(),
            ViewLink = ("view cab",
                Url.RouteUrl(CABProfileController.Routes.CabDetails,
                    workFlowTask.CABId)),
            CompletedBy = "completed by",
            AssignedOn =
                workFlowTask.Assigned != null ? workFlowTask?.Assigned.Value.ToShortDateString() : string.Empty,
            SelectAssignee = await GetUser(),
            SelectedAssignee = workFlowTask.Assignee?.FirstName! + " " + workFlowTask.Assignee?.Surname,
            SelectedAssigneeId = workFlowTask.Assignee?.UserId,
            UserGroup = "BPSS"
        };

        return View(vm);
    }

//todo : Post needs to be implement
    [HttpPost("details/{id}", Name = Routes.NotificationDetails)]
    public async Task<IActionResult> Detail(string id, NotificationDetailViewModel model)
    {
        var workFlowTask = await _workflowTaskService.GetAsync(Guid.Parse(id));

        model.NotificationTitle = "Notification Details";
        model.Status = WorkFlowTaskStatus(workFlowTask.Completed, workFlowTask.Assignee);
        model.From = workFlowTask.Submitter.FirstName + " " + workFlowTask.Submitter.Surname;
        model.Subject = workFlowTask.TaskType.ToString();
        model.Reason = workFlowTask.Reason;
        model.SentOn = workFlowTask.SentOn.ToShortDateString(); //TODO: create helper for UK
        model.CompletedOn = "todo"; //TODO : check with date
        model.LastUpdated = workFlowTask.LastUpdatedOn.ToShortDateString();
        model.ViewLink = ("view cab", "/");
        model.CompletedBy = "completed by";
        model.AssignedOn = workFlowTask.Assigned != null
            ? workFlowTask.Assigned.Value.ToShortDateString()
            : string.Empty;
        model.SelectAssignee = await GetUser();
        model.SelectedAssignee = model.SelectedAssignee;
        model.UserGroup = "BPSS";

        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        var userAccount = await _userService.GetAsync(model.SelectedAssignee);

        workFlowTask.Assignee =    new User(model.SelectedAssignee, userAccount.FirstName, userAccount.Surname, userAccount.Role);
        workFlowTask.Assigned = DateTime.Now;
        await _workflowTaskService.UpdateAsync(workFlowTask);

        return RedirectToRoute(Routes.NotificationsHome);
    }

    private async Task<List<(string, string)>> GetUser()
    {
        var options =
            new UserAccountListOptions(SkipTake.FromPage(-1, 40), new SortBy("firstName", null), false, null, null);
        var role = User.IsInRole(Roles.OPSS.Id) ? Roles.OPSS.Id : Roles.UKAS.Id;
        var users = await _userService.ListAsync(options);
        var assignees = users.Where(x => x.Role == role)
            .Select(user => new ValueTuple<string, string>(user.Id, user.FirstName! + " " + user.Surname)).ToList();
        return assignees;
    }

    private string WorkFlowTaskStatus(bool isCompleted, User? assignee)
    {
        return isCompleted ? "Completed" :
            assignee == null ? "Unassigned" : "Assigned";
    }
}