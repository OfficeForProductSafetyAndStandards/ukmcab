using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.Notification;

public record NotificationDetailViewModel : ILayoutModel  
{
 
    public string? SelectedStatus { get; set; }
    public string? Status { get; set; }
    public bool IsCompleted { get; set; } = false;
    public bool IsAssigned { get; set; } = false;
    public string? From { get; set; }
    public string? Subject { get; set; }
    public string? Reason { get; set; }
    public string? SentOn { get; set; }
    public string? CompletedOn { get; set; }
    public string? LastUpdated { get; set; }
    public (string? ViewLinkName, string? ViewLinkAddress) ViewLink { get; set; }
    public string? CompletedBy { get; set; }
    public string? AssignedOn { get; set; }
    public List<(string Value, string Text)>? SelectAssignee { get; set; }
    
    [Required(ErrorMessage = "Select an assignee")]
    public string SelectedAssignee { get; set; }
    public string? SelectedAssigneeId { get; set; }
    public string? UserGroup { get; set; }
    public string? Title { get; }
}