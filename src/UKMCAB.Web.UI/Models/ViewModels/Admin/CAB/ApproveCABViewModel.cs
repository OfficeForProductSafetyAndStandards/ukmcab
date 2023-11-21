using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;

public record ApproveCABViewModel(string? Title, string CABId, string CABName, string CABNumber) : BasicPageModel(Title)
{
    [Required]
    public string CABNumber { get; set; } = CABNumber;
} 
