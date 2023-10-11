using System.Collections;
using UKMCAB.Web.UI.Models.ViewModels.Shared;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.Notification;

public record NotificationDetailViewModel(
    string PageTitle, 
    string NotificationTitle, 
    string SelectedStatus,
    List<(int Value, string Text)> SelectStatus,
    string From, 
    string Subject,
    string Reason,
    string SentOn,
    string CompletedOn,
    (string ViewLinkName,string ViewLinkAddress) ViewLink,
    string CompletedBy,
    string AssignedOn,
    List<(string Value, string Text)> SelectAssignee,
    string SelectedAssignee,
    string UserGroup
    ) : BasicPageModel(PageTitle);
