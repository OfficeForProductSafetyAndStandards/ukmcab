using UKMCAB.Web.UI.Models.ViewModels.Shared;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.Notification;

public record NotificationsViewModelTab(bool ShowTableItems, string SortField, string SortDirection,
    IEnumerable<(string From, string Subject, string CABName, string SentOn, string? DetailLink)> Items,
    PaginationViewModel Pagination, MobileSortTableViewModel MobileSortTableViewModel);