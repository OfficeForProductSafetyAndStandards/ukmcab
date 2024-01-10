using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;

public record ApproveCABViewModel(string? title, string cabName) : BasicPageModel(title)
{
    [Required(ErrorMessage = "Enter a CAB number", AllowEmptyStrings = false)]
    public string CABNumber { get; set; } = null!;

    [Required(ErrorMessage = "Select who should see the CAB number", AllowEmptyStrings = false)]
    public string CabNumberVisibility { get; set; } = null!;

    public string CabName { get; set; } = cabName;

} 
