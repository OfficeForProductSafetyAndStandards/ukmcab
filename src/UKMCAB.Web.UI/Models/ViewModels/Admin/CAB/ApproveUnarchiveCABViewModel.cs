using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;

public record ApproveUnarchiveCABViewModel(string? Title, string CABName, string CabUrl, Guid CabId, string SubmitterGroup, string SubmitterFirstAndLastName, bool PublishRequested) : BasicPageModel(Title)
{
    [Required(ErrorMessage = "Select an option")]
    public bool? IsPublish { get; set; }
} 
