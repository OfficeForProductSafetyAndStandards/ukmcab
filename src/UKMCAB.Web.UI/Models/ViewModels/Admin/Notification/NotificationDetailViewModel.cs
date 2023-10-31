using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.Notification;

public record NotificationDetailViewModel(
    string PageTitle, 
    string NotificationTitle, 
    string SelectedStatus,
    string Status,
    string From, 
    string Subject,
    string Reason,
    string SentOn,
    string CompletedOn,
    string LastUpdated,
    (string ViewLinkName,string ViewLinkAddress) ViewLink,
    string CompletedBy,
    string AssignedOn,
    List<(string Value, string Text)> SelectAssignee,
    string SelectedAssignee,
    string UserGroup
    ) : BasicPageModel(PageTitle);
