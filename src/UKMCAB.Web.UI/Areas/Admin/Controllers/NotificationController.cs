using System.Globalization;
using System.Security.Claims;
using UKMCAB.Core.Services.Users;
using Microsoft.AspNetCore.Authorization;
using UKMCAB.Core.Domain.Workflow;
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
        public const string Notifications = "admin.notifications";
    }

    private readonly IWorkflowTaskService _workflowTaskService;
    private readonly ICABAdminService _cabAdminService;

    public NotificationController(IWorkflowTaskService workflowTaskService, ICABAdminService cabAdminService)
    {
        _workflowTaskService = workflowTaskService;
        _cabAdminService = cabAdminService;
    }

    [HttpGet(Name = Routes.Notifications)]
    public async Task<IActionResult> Index(string sf, string sd, int pageNumber = 1)
    {
        var model = await CreateNotificationsViewModelAsync(sf, sd, pageNumber);
        return View(model);
    }

    private async Task<NotificationsViewModel> CreateNotificationsViewModelAsync(
        string sf,
        string sd,
        int pageNumber)
    {
        if (string.IsNullOrWhiteSpace(sd))
        {
            sd = SortDirectionHelper.Ascending;
        }

        var role = User.IsInRole(Roles.OPSS.Id) ? Roles.OPSS.Id : Roles.UKAS.Id;
        var unassigned = await _workflowTaskService.GetUnassignedByForRoleIdAsync(role);
        var assignedToMe = await _workflowTaskService.GetByAssignedUserAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var assignedToGroup = await _workflowTaskService.GetUnassignedByForRoleIdAsync(role);
        var completed = await _workflowTaskService.GetByAssignedUserRoleAndCompletedAsync(role,true);
        List<(string From, string Subject, string CABName, string SentOn, string? CABLink)> unAssignedItems =
            await BuildItems(unassigned);
        List<(string From, string Subject, string CABName, string SentOn, string? CABLink)> assignedToMeItems =
            await BuildItems(assignedToMe);
        List<(string From, string Subject, string CABName, string SentOn, string? CABLink)> assignedToGroupItems =
            await BuildItems(assignedToGroup);
        List<(string From, string Subject, string CABName, string SentOn, string? CABLink)> completedItems =
            await BuildItems(completed);

        var model = new NotificationsViewModel
        (
            Constants.PageTitle.Notifications,
            new NotificationsViewModelTab(unAssignedItems.Any(), sf, sd, unAssignedItems, new PaginationViewModel
            {
                PageNumber = pageNumber,
                ResultsPerPage = 5,
                Total = unAssignedItems.Count
            }, new MobileSortTableViewModel(sf, SortDirectionHelper.Get(sd), new List<Tuple<string, string>>
            {
                new("From", "From")
            })),
            new NotificationsViewModelTab(assignedToMeItems.Any(), sf, SortDirectionHelper.Get(sd), assignedToMeItems,
                new PaginationViewModel
                {
                    PageNumber = pageNumber,
                    ResultsPerPage = 5,
                    Total = unAssignedItems.Count
                }, new MobileSortTableViewModel(sf, sd, new List<Tuple<string, string>>
                {
                    new("From", "From")
                })),
            new NotificationsViewModelTab(assignedToGroupItems.Any(), sf, SortDirectionHelper.Get(sd),
                assignedToGroupItems, new PaginationViewModel
                {
                    PageNumber = pageNumber,
                    ResultsPerPage = 5,
                    Total = unAssignedItems.Count
                }, new MobileSortTableViewModel(sf, SortDirectionHelper.Get(sd), new List<Tuple<string, string>>
                {
                    new("From", "From")
                })),
            new NotificationsViewModelTab(completedItems.Any(), sf, SortDirectionHelper.Get(sd), completedItems,
                new PaginationViewModel
                {
                    PageNumber = pageNumber,
                    ResultsPerPage = 5,
                    Total = unAssignedItems.Count
                }, new MobileSortTableViewModel(sf, SortDirectionHelper.Get(sd), new List<Tuple<string, string>>
                {
                    new("From", "From")
                }))
        );
        return model;
    }

    private async Task<List<(string From, string Subject, string CABName, string SentOn, string? CABLink)>> BuildItems(
        List<WorkflowTask> unassigned)
    {
        var unassigneditems = new List<(string From, string Subject, string CABName, string SentOn, string? CABLink)>();
        foreach (var notification in unassigned)
        {
            var cab = await _cabAdminService.GetLatestDocumentAsync(notification.CABId.ToString());
            var item = (From: notification.Submitter.FirstAndLastName, Subject: notification.TaskType.ToString(),
                CABName: cab.Name, SentOn: notification.SentOn.ToString(CultureInfo.CurrentCulture),
                CABLink: Url.RouteUrl(NotificationDetailsController.Routes.NotificationDetails, notification.Id));
            unassigneditems.Add(item);
        }

        return unassigneditems;
    }
}