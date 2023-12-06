using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using UKMCAB.Common.Extensions;
using UKMCAB.Core.Domain.CAB;
using UKMCAB.Core.Domain.Workflow;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Workflow;
using UKMCAB.Data.Domain;
using UKMCAB.Data.Models;
using UKMCAB.Infrastructure.Cache;
using UKMCAB.Web.UI.Models.ViewModels.Admin.Notification;
using UKMCAB.Web.UI.Models.ViewModels.Shared;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers;

[Area("admin"), Route("admin/notifications"), Authorize]
public class NotificationController : Controller
{
    public const string AssignedToGroupTabName = "assigned-group";
    public const string LastUpdatedLabel = "Last updated";

    private const string UnassignedTabName = "unassigned";
    private const string AssignedToMeTabName = "assigned-me";
    private const string CompletedTabName = "completed";

    private const string LastUpdated = "LastUpdated";
    private const string From = "From";
    private const string Subject = "Subject";
    private const string Assignee = "Assignee";
    private const string CabNameValue = "CABName";
    private const string CABNameLabel = "CAB name";
    private const string SentOnLabel = "Sent on";
    private const string CompletedOn = "Completed on";

    public static class Routes
    {
        public const string Notifications = "admin.notifications";
    }

    private readonly IWorkflowTaskService _workflowTaskService;
    private readonly ICABAdminService _cabAdminService;
    private readonly IDistCache _distCache;
    
    public NotificationController(IWorkflowTaskService workflowTaskService, ICABAdminService cabAdminService,
        IDistCache distCache)
    {
        _workflowTaskService = workflowTaskService;
        _cabAdminService = cabAdminService;
        _distCache = distCache;
    }

    [HttpGet(Name = Routes.Notifications)]
    public async Task<IActionResult> Index(string sf, string sd, int? pageNumber = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var assignedToMe = await _workflowTaskService.GetByAssignedUserAsync(userId);
        var cacheKey = userId + "-assigned-to-me-notifications";
        if (assignedToMe.Any() && await _distCache.GetAsync<string?>(cacheKey) == null &&
            string.IsNullOrWhiteSpace(sf) && string.IsNullOrWhiteSpace(sd) && pageNumber == null)
        {
            //Business rule redirect to assigned to me tab if items found
            await _distCache.SetAsync(cacheKey, assignedToMe.Count.ToString(), new TimeSpan(0, 0, 10, 0));
            return new RedirectResult(Url.RouteUrl(Routes.Notifications) + $"#{AssignedToMeTabName}");
        }

        await _distCache.RemoveAsync(cacheKey);
        var model = await CreateNotificationsViewModelAsync(assignedToMe, sf, sd, pageNumber ?? 1,userId);
        ModelState.Clear();
        return View(model);
    }

    private async Task<NotificationsViewModel> CreateNotificationsViewModelAsync(
        List<WorkflowTask> assignedToMe,
        string sf,
        string sd,
        int pageNumber,
        string? userId
        )
    {
        if (string.IsNullOrWhiteSpace(sf) && string.IsNullOrWhiteSpace(sd))
        {
            sf = LastUpdated;
            sd = SortDirectionHelper.Descending;
        }

        var role = User.IsInRole(Roles.OPSS.Id) ? Roles.OPSS : Roles.UKAS;
        var resultsPerPage = 5;
        var skipTake = SkipTake.FromPage(pageNumber - 1, 5);

        var unassigned = await _workflowTaskService.GetUnassignedByForRoleIdAsync(role.Id);
        var assignedToGroup =
            await _workflowTaskService.GetAssignedToGroupForRoleIdAsync(role.Id, userId);
        var completed =
            await _workflowTaskService.GetCompletedForRoleIdAsync(role.Id);

        List<(string From, string Subject, string? CABName, string? Assignee, DateTime LastUpdated, string? DetailLink)>
            unAssignedItems =
                await BuildTableItemsAsync(unassigned, sf, sd);
        List<(string From, string Subject, string? CABName, string? Assignee, DateTime LastUpdated, string? DetailLink)>
            assignedToMeItems =
                await BuildTableItemsAsync(assignedToMe, sf, sd);
        List<(string From, string Subject, string? CABName, string? Assignee, DateTime LastUpdated, string? DetailLink)>
            assignedToGroupItems =
                await BuildTableItemsAsync(assignedToGroup, sf, sd);
        List<(string From, string Subject, string? CABName, string? Assignee, DateTime LastUpdated, string? DetailLink)>
            completedItems =
                await BuildTableItemsAsync(completed, sf, sd);


        var model = new NotificationsViewModel
        (
            Constants.PageTitle.Notifications,
            new NotificationsViewModelTable(unAssignedItems.Any(), sf, sd,
                unAssignedItems.Skip(skipTake.Skip).Take(skipTake.Take), new PaginationViewModel
                {
                    PageNumber = pageNumber,
                    ResultsPerPage = resultsPerPage,
                    Total = unAssignedItems.Count,
                    TabId = UnassignedTabName
                },
                new MobileSortTableViewModel(sf, SortDirectionHelper.Descending,
                    BuildMobileSortOptions(UnassignedTabName, sf)),
                UnassignedTabName,
                "unassigned", unAssignedItems.Count, SentOnLabel),
            new NotificationsViewModelTable(assignedToMeItems.Any(), sf, SortDirectionHelper.Get(sd),
                assignedToMeItems.Skip(skipTake.Skip).Take(skipTake.Take),
                new PaginationViewModel
                {
                    PageNumber = pageNumber,
                    ResultsPerPage = resultsPerPage,
                    Total = assignedToMe.Count,
                    TabId = AssignedToMeTabName
                },
                new MobileSortTableViewModel(sf, SortDirectionHelper.Descending,
                    BuildMobileSortOptions(AssignedToMeTabName, sf)),
                AssignedToMeTabName, "assigned to me",
                assignedToMe.Count, LastUpdatedLabel),
            new NotificationsViewModelTable(assignedToGroupItems.Any(), sf, SortDirectionHelper.Get(sd),
                assignedToGroupItems.Skip(skipTake.Skip).Take(skipTake.Take), new PaginationViewModel
                {
                    PageNumber = pageNumber,
                    ResultsPerPage = resultsPerPage,
                    Total = assignedToGroup.Count,
                    TabId = AssignedToGroupTabName
                },
                new MobileSortTableViewModel(sf, SortDirectionHelper.Descending,
                    BuildMobileSortOptions(AssignedToGroupTabName, sf)),
                AssignedToGroupTabName, "assigned to " + role.Label, assignedToGroup.Count),
            new NotificationsViewModelTable(completedItems.Any(), sf, SortDirectionHelper.Get(sd),
                completedItems.Skip(skipTake.Skip).Take(skipTake.Take),
                new PaginationViewModel
                {
                    PageNumber = pageNumber,
                    ResultsPerPage = resultsPerPage,
                    Total = completed.Count,
                    TabId = CompletedTabName
                },
                new MobileSortTableViewModel(sf, SortDirectionHelper.Descending,
                    BuildMobileSortOptions(CompletedTabName, sf)),
                CompletedTabName,
                "completed", completedItems.Count, CompletedOn),
            role.Label
        );
        return model;
    }

    private List<Tuple<string, string, bool>> BuildMobileSortOptions(string tabName, string sortField)
    {
        var items = new Dictionary<string, Tuple<string, string, bool>>();
        items.Add(From, new(From, From, false));
        items.Add(Subject, new(Subject, Subject, false));
        items.Add(CabNameValue, new(CabNameValue, CABNameLabel, false));

        switch (tabName)
        {
            case AssignedToMeTabName:
                items.Add(LastUpdated, new Tuple<string, string, bool>(LastUpdated, LastUpdatedLabel, false));
                break;
            case UnassignedTabName:
                items.Add(LastUpdated, new(LastUpdated, SentOnLabel, false));
                break;
            case AssignedToGroupTabName:
                items.Add(Assignee, new(Assignee, Assignee, false));
                items.Add(LastUpdated, new(LastUpdated, LastUpdatedLabel, false));
                break;
            case CompletedTabName:
                items.Add(LastUpdated, new(LastUpdated, CompletedOn, false));
                break;
        }

        if (items.ContainsKey(sortField))
        {
            var selectedItem = items[sortField];
            items[sortField] = new Tuple<string, string, bool>(selectedItem.Item1, selectedItem.Item2, true);
        }

        return items.Select(i => i.Value).ToList();
    }

    private async Task<List<(string From, string Subject, string? CABName, string? Assignee, DateTime LastUpdated, string?
            DetailLink)>>
        BuildTableItemsAsync(
            IEnumerable<WorkflowTask> tasks,
            string? sf,
            string? sd)
    {
        var items =
            new List<(string From, string Subject, string? CABName, string? Assignee, DateTime LastUpdated, string?
                DetailLink)>();
        foreach (var notification in tasks)
        {
            string? cabName = null;
            if (notification.CABId.HasValue)
            {
                var cabs = await _cabAdminService.FindDocumentsByCABIdAsync(notification.CABId.ToString()!);
                cabName = cabs.FirstOrDefault()?.Name;
            }

            var item = (From: notification.Submitter.FirstAndLastName,
                Subject: notification.TaskType.GetEnumDescription(),
                CABName: cabName,
                Assignee: notification.Assignee?.FirstAndLastName,
                LastUpdated: notification.SentOn,
                DetailLink: Url.RouteUrl(NotificationDetailsController.Routes.NotificationDetails,
                    new { id = notification.Id.ToString() }));
            items.Add(item);
        }

        if (!items.Any())
        {
            return items;
        }

        // ReSharper disable once RedundantAssignment
        // ReSharper disable once EntityNameCapturedOnly.Local
        var itemForNameOf = items.First();
        switch (sf?.ToLower())
        {
            case { } str when str.Equals(nameof(itemForNameOf.From), StringComparison.CurrentCultureIgnoreCase):
                return sd == SortDirectionHelper.Ascending
                    ? items.OrderBy(i => i.From).ToList()
                    : items.OrderByDescending(i => i.From).ToList();
            case { } str when str.Equals(nameof(itemForNameOf.Subject), StringComparison.CurrentCultureIgnoreCase):
                return sd == SortDirectionHelper.Ascending
                    ? items.OrderBy(i => i.Subject).ToList()
                    : items.OrderByDescending(i => i.Subject).ToList();
            case { } str when str.Equals(nameof(itemForNameOf.CABName), StringComparison.CurrentCultureIgnoreCase):
                return sd == SortDirectionHelper.Ascending
                    ? items.OrderBy(i => i.CABName).ToList()
                    : items.OrderByDescending(i => i.CABName).ToList();
            case { } str when str.Equals(nameof(itemForNameOf.Assignee), StringComparison.CurrentCultureIgnoreCase):
                return sd == SortDirectionHelper.Ascending
                    ? items.OrderBy(i => i.Assignee).ToList()
                    : items.OrderByDescending(i => i.Assignee).ToList();
            case { } str when str.Equals(nameof(itemForNameOf.LastUpdated), StringComparison.CurrentCultureIgnoreCase):
                return sd == SortDirectionHelper.Ascending
                    ? items.OrderBy(i => i.LastUpdated).ToList()
                    : items.OrderByDescending(i => i.LastUpdated).ToList();
            default:
                return items.OrderByDescending(i => i.LastUpdated).ToList();
        }
    }
}