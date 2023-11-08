using UKMCAB.Web.UI.Models.ViewModels.Shared;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.Notification;

public record NotificationsViewModelTable(bool ShowTableItems, string SortField, string SortDirection,
    IEnumerable<(string From, string Subject, string CABName, string LastUpdated, string? DetailLink)> Items,
    PaginationViewModel Pagination, MobileSortTableViewModel MobileSortTableViewModel, string TabName, string NoItemsLabel);