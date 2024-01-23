using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Search.RequestToUnpublishCAB;

public record RequestToUnpublishCABViewModel(string CabName, string CabUrl, Guid CabId) : BasicPageModel
{
    [Required(ErrorMessage = "Select an option")]
    public bool? IsUnpublish { get; set; }
    
    [Required(ErrorMessage = "Enter the reason for requesting to unpublish this CAB")]
    [MaxLength(ErrorMessage = "Maximum reason length is 1000 characters")]
    public string? Reason { get; set; }
}