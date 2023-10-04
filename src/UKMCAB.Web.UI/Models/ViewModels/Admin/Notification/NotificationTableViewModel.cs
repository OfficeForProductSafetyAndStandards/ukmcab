using System.Collections;
using UKMCAB.Web.UI.Models.ViewModels.Shared;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.Notification;

public record NotificationTableViewModel(bool ShowHeader, string SortField, string SortDirection, IEnumerable<(string
    From, string Subject, string CABName, string SentOn, string CABLink)> Items, PaginationViewModel Pagination);