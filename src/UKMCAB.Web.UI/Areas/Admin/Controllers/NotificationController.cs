using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using UKMCAB.Common.Extensions;
using UKMCAB.Core.Domain.Workflow;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.Users;
using UKMCAB.Core.Services.Workflow;
using UKMCAB.Data.CosmosDb.Services.CachedCAB;
using UKMCAB.Data.Domain;
using UKMCAB.Infrastructure.Cache;
using UKMCAB.Web.UI.Models.ViewModels.Admin.Notification;
using UKMCAB.Web.UI.Models.ViewModels.Shared;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers;

[Area("admin"), Route("admin/notifications"), Authorize]
public class NotificationController : UI.Controllers.ControllerBase
{
    public const string AssignedToGroupTabName = "assigned-group";
    public const string LastUpdatedLabel = "Last updated";

    public const string UnassignedTabName = "unassigned";
    public const string AssignedToMeTabName = "assigned-me";
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
    private readonly IDistCache _distCache;
    private readonly ICachedPublishedCABService _cachedPublishedCABService;

    public NotificationController(IWorkflowTaskService workflowTaskService,
        IDistCache distCache,
        ICachedPublishedCABService cachedPublishedCABService, IUserService userService) 
        : base(userService)
    {
        _workflowTaskService = workflowTaskService;
        _distCache = distCache;
        _cachedPublishedCABService = cachedPublishedCABService;
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

        var skipTake = SkipTake.FromPage(pageNumber - 1, Constants.RowsPerPage);

        var unassignedTask = _workflowTaskService.GetUnassignedByForRoleIdAsync(UserRoleId);
        var assignedToGroupTask =
            _workflowTaskService.GetAssignedToGroupForRoleIdAsync(UserRoleId, userId);
        var completedTask =
            _workflowTaskService.GetCompletedForRoleIdAsync(UserRoleId);

        Task<List<(string From, string Subject, string? CABName, string? Assignee, DateTime LastUpdated, string? DetailLink)>>
            unAssignedItemsTask =
                BuildTableItemsAsync(await unassignedTask, sf, sd);
        Task<List<(string From, string Subject, string? CABName, string? Assignee, DateTime LastUpdated, string? DetailLink)>>
            assignedToMeItemsTask =
                BuildTableItemsAsync(assignedToMe, sf, sd);
        Task<List<(string From, string Subject, string? CABName, string? Assignee, DateTime LastUpdated, string? DetailLink)>>
            assignedToGroupItemsTask =
                BuildTableItemsAsync(await assignedToGroupTask, sf, sd);
        Task<List<(string From, string Subject, string? CABName, string? Assignee, DateTime LastUpdated, string? DetailLink)>>
            completedItemsTask =
                BuildTableItemsAsync(await completedTask, sf, sd);

        var model = new NotificationsViewModel
        (
            Constants.PageTitle.Notifications,
            new NotificationsViewModelTable((await unAssignedItemsTask).Any(), sf, sd,
                (await unAssignedItemsTask).Skip(skipTake.Skip).Take(skipTake.Take), new PaginationViewModel
                {
                    PageNumber = pageNumber,
                    ResultsPerPage = Constants.RowsPerPage,
                    Total = (await unAssignedItemsTask).Count,
                    TabId = UnassignedTabName
                },
                new MobileSortTableViewModel(sf, SortDirectionHelper.Descending,
                    BuildMobileSortOptions(UnassignedTabName, sf)),
                UnassignedTabName,
                $"There are no notifications assigned to {Roles.NameFor(UserRoleId)}", 
                (await unAssignedItemsTask).Count, SentOnLabel),
            new NotificationsViewModelTable((await assignedToMeItemsTask).Any(), sf, SortDirectionHelper.Get(sd),
                (await assignedToMeItemsTask).Skip(skipTake.Skip).Take(skipTake.Take),
                new PaginationViewModel
                {
                    PageNumber = pageNumber,
                    ResultsPerPage = Constants.RowsPerPage,
                    Total = assignedToMe.Count,
                    TabId = AssignedToMeTabName
                },
                new MobileSortTableViewModel(sf, SortDirectionHelper.Descending,
                    BuildMobileSortOptions(AssignedToMeTabName, sf)),
                AssignedToMeTabName, "There are no notifications assigned to you",
                assignedToMe.Count, LastUpdatedLabel),
            new NotificationsViewModelTable((await assignedToGroupItemsTask).Any(), sf, SortDirectionHelper.Get(sd),
                (await assignedToGroupItemsTask).Skip(skipTake.Skip).Take(skipTake.Take), new PaginationViewModel
                {
                    PageNumber = pageNumber,
                    ResultsPerPage = Constants.RowsPerPage,
                    Total = (await assignedToGroupItemsTask).Count,
                    TabId = AssignedToGroupTabName
                },
                new MobileSortTableViewModel(sf, SortDirectionHelper.Descending,
                    BuildMobileSortOptions(AssignedToGroupTabName, sf)),
                AssignedToGroupTabName, $"There are no notifications assigned to another {Roles.NameFor(UserRoleId)} user",
                (await assignedToGroupTask).Count),
            new NotificationsViewModelTable((await completedItemsTask).Any(), sf, SortDirectionHelper.Get(sd),
                (await completedItemsTask).Skip(skipTake.Skip).Take(skipTake.Take),
                new PaginationViewModel
                {
                    PageNumber = pageNumber,
                    ResultsPerPage = Constants.RowsPerPage,
                    Total = (await completedItemsTask).Count,
                    TabId = CompletedTabName
                },
                new MobileSortTableViewModel(sf, SortDirectionHelper.Descending,
                    BuildMobileSortOptions(CompletedTabName, sf)),
                CompletedTabName,
                "There are no completed notifications", (await completedItemsTask).Count, CompletedOn),
            Roles.NameFor(UserRoleId)
        );
        return model;
    }

    private List<Tuple<string, string, bool>> BuildMobileSortOptions(string tabName, string sortField)
    {
        var items = new Dictionary<string, Tuple<string, string, bool>>
        {
            { From, new(From, From, false) },
            { Subject, new(Subject, Subject, false) },
            { CabNameValue, new(CabNameValue, CABNameLabel, false) }
        };

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
                 var cabs = await _cachedPublishedCABService.FindAllDocumentsByCABIdAsync(notification.CABId.ToString()!);
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