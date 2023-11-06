using System.Collections;
using UKMCAB.Web.UI.Models.ViewModels.Shared;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.Notification;

public record NotificationsViewModel(string PageTitle, NotificationsViewModelTab UnassignedTab,NotificationsViewModelTab AssignedToMeTab,NotificationsViewModelTab AssignedToGroupTab,NotificationsViewModelTab CompletedTab) : BasicPageModel(PageTitle);

public record NotificationsViewModelTab(bool ShowTableItems, string SortField, string SortDirection,
    IEnumerable<(string From, string Subject, string CABName, string SentOn, string? CABLink)> UnassignedItems,
    PaginationViewModel Pagination, MobileSortTableViewModel MobileSortTableViewModel);
