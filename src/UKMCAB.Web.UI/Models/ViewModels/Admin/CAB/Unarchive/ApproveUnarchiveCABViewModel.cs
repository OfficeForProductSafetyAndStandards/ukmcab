using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Unarchive;

public record ApproveUnarchiveCABViewModel(string? Title, string CABName, string CabUrl, Guid CabId, string SubmitterGroup, string SubmitterFirstAndLastName, bool PublishRequested) : BasicPageModel(Title)
{
    [Required(ErrorMessage = "Select an option")]
    public bool? IsPublish { get; set; }
    
    [MaxLength(1000, ErrorMessage = "Maximum reason length is 1000 characters")]
    public string? Reason { get; set; }

    [MaxLength(1000, ErrorMessage = "Maximum user notes length is 1000 characters")]
    public string? UserNotes { get; set; }
}