using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Search.RequestToUnarchiveCAB;

public record RequestToUnarchiveCABViewModel(string CABName, Guid CABId) : BasicPageModel
{
    [Required(ErrorMessage = "Select an option")]
    public bool? IsPublish { get; set; }
    
    [Required(ErrorMessage = "Enter the reason for requesting to unarchive this CAB")]
    public string? Reason { get; set; }
}