using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;

public record DeclineCABViewModel(string? Title, string CabName) : BasicPageModel(Title)
{
    public string CABName { get; set; } = CabName;
    [Required(ErrorMessage = "Enter the reason for declining this CAB profile", AllowEmptyStrings = false)]
    public string DeclineReason { get; set; } = null!;
} 
