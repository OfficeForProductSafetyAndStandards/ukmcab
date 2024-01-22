using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Search.RequestToUnarchiveCAB;

public record RequestToUnarchiveCABViewModel(string CABName, string CabUrl, Guid CabId) : BasicPageModel
{
    [Required(ErrorMessage = "Select an option")]
    public bool? IsPublish { get; set; }
    
    [Required(ErrorMessage = "Enter the reason for requesting to unarchive this CAB")]
    [MaxLength(ErrorMessage = "Maximum reason length is 1000 characters")]
    public string? Reason { get; set; }
}