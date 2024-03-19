using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;

public record DeclineLAViewModel(string? Title, Guid CabId) : BasicPageModel(Title)
{
    [Required(ErrorMessage = "Enter the reason for declining this legislative area", AllowEmptyStrings = false)]
    public string DeclineReason { get; set; } = null!;
} 
