using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;

public record ApproveCABViewModel(string? Title, string CABName) : BasicPageModel(Title)
{
    [Required(ErrorMessage = "Enter a CAB number", AllowEmptyStrings = false)]
    public string CABNumber { get; set; } = null!;
} 
