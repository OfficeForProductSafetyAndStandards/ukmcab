using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Unpublish;

public record DeclineUnpublishCABViewModel(string? Title, string CABName, string CabUrl, Guid CabId, string SubmitterGroup) : BasicPageModel(Title)
{
    [Required(ErrorMessage = "Enter the reason for declining the request to unpublish this CAB")]
    [MaxLength(1000, ErrorMessage = "Maximum reason length is 1000 characters")]
    public string? Reason { get; set; }
}