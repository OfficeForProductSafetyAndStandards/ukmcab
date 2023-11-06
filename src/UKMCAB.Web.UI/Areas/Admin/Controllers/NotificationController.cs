using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using UKMCAB.Core.Domain.Workflow;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Workflow;
using UKMCAB.Subscriptions.Core.Integration.CabService;
using UKMCAB.Web.UI.Models.ViewModels.Admin.Notification;
using UKMCAB.Web.UI.Models.ViewModels.Shared;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers;

[Area("admin"), Route("admin/notifications"), Authorize]
public class NotificationController : Controller
{
    public static class Routes
    {
        public const string NotificationsUnassigned = "admin.notifications";
        public const string NotificationsAssignedToMe = "admin.notifications.assigned.me";
        public const string NotificationsAssignedToMyGroup = "admin.notifications.assigned.group";
        public const string NotificationsCompleted = "admin.notifications.completed";
        public const string NotificationDetails = "admin.notification.details";
    }

    private readonly IWorkflowTaskService _workflowTaskService;
    private readonly ICABAdminService _cabAdminService;
    private string? _role;

   
    public NotificationController(IWorkflowTaskService workflowTaskService, ICABAdminService cabAdminService)
    {
        _workflowTaskService = workflowTaskService;
        _cabAdminService = cabAdminService;
    }


    [HttpGet(Name = Routes.NotificationsUnassigned)]
    public async Task<IActionResult> Index(string sf, string sd, int pageNumber = 1)
    {
        var test = Request.Path;
        var testBase = Request.PathBase;
        _role = User.IsInRole(Roles.OPSS.Id) ? Roles.OPSS.Id : Roles.UKAS.Id;
        var unassignedNotifications = await _workflowTaskService.GetUnassignedBySubmittedUserRoleAsync(_role);
        var model = await CreateNotificationsViewModelAsync(sf, sd, pageNumber, unassignedNotifications);
        return View(model);
    }

    [HttpGet("assigned-to-me", Name = Routes.NotificationsAssignedToMe)]
    public async Task<IActionResult> AssignedToMe(string sf, string sd, int pageNumber = 1)
    {
        _role = User.IsInRole(Roles.OPSS.Id) ? Roles.OPSS.Id : Roles.UKAS.Id;
        var assignedToMe = await _workflowTaskService.GetByAssignedUserAsync(_role);
        var model = await CreateNotificationsViewModelAsync(sf, sd, pageNumber, assignedToMe);
        return Redirect(Url.RouteUrl(NotificationController.Routes.NotificationsUnassigned) + "#assigned-me" );

        return View("~/Areas/Admin/Views/Notification/Index.cshtml", model);
    }

    [HttpGet("/assigned-to-group", Name = Routes.NotificationsAssignedToMyGroup)]
    public async Task<IActionResult> AssignedToGroup(string sf, string sd, int pageNumber = 1)
    {
        _role = User.IsInRole(Roles.OPSS.Id) ? Roles.OPSS.Id : Roles.UKAS.Id;
        var assignedToGroupPending = await _workflowTaskService.GetByAssignedUserRoleAndCompletedAsync(_role);
        var model = await CreateNotificationsViewModelAsync(sf, sd, pageNumber, assignedToGroupPending);
        return View("~/Areas/Admin/Views/Notification/Index.cshtml", model);
    }

    private async Task<NotificationsViewModel> CreateNotificationsViewModelAsync(string sf, string sd, int pageNumber,
        List<WorkflowTask> unassignedNotifications)
    {
        var items = new List<(string From, string Subject, string CABName, string SentOn, string? CABLink)>();
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
        return model;
    }

    [HttpGet("details/{id}", Name = Routes.NotificationDetails)]
    public IActionResult Detail(string id)
    {
        //todo connect to service
        var status = "Assigned"; // Unassigned,  Assigned, Completed 
        var assignees = new List<(string Value, string Text)>
        {
            ("user1", "Test User 1"),
            ("user2", "Test User 2")
        };


        var vm = new NotificationDetailViewModel(
            "Notification Details",
            "Notification: Test Notification",
            "1",
            Status: status,
            "From value",
            "Subject value",
            "reason value",
            "11/10/2023 12:15",
            "12/10/2023 13:00",
            "23/10/2023 15:00",
            ("view cab", "/"),
            "Mr BPSS",
            "12/10/2023 11:00",
            assignees,
            assignees.First().Value, "BPSS"
        );
        return View(vm);
    }

    //todo : Post needs to be implement
    [HttpPost("details/{id}", Name = Routes.NotificationDetails)]
    public async Task<IActionResult> Detail(string id, NotificationDetailViewModel model)
    {
        if (ModelState.IsValid)
        {
        }

        return View(model);
    }
}