using System.Globalization;
using UKMCAB.Core.Services.Users;
using Microsoft.AspNetCore.Authorization;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Workflow;
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
                CABLink: Url.RouteUrl(NotificationDetailsController.Routes.NotificationDetails, notification.Id));
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
}