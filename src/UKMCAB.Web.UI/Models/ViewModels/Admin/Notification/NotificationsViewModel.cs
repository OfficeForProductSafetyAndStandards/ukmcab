namespace UKMCAB.Web.UI.Models.ViewModels.Admin.Notification;

public record NotificationsViewModel(string PageTitle, NotificationsViewModelTable UnassignedTable,
    NotificationsViewModelTable AssignedToMeTable, NotificationsViewModelTable AssignedToGroupTable,
    NotificationsViewModelTable CompletedTable, string UserGroup) : BasicPageModel(PageTitle);