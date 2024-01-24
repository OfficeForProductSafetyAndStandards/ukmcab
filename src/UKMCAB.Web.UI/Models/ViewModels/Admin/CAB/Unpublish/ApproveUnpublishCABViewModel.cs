using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Unpublish;

public record ApproveUnpublishCABViewModel(
    string? Title,
    string CABName,
    string CabUrl,
    Guid CabId,
    bool ShowUserNotesField) : BasicPageModel(Title)
{
    [Required(ErrorMessage = "Enter the reason for approving the request to unpublish this CAB")]
    [MaxLength(1000, ErrorMessage = "Maximum reason length is 1000 characters")]
    public string? Reason { get; set; }

    [MaxLength(1000, ErrorMessage = "Maximum user notes length is 1000 characters")]
    public string? UserNotes { get; set; }
}