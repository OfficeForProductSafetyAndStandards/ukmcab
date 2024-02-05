using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Shared;
using System.ComponentModel.DataAnnotations;
namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;

public record LegislativeAreaAdditionalInformationViewModel(
    string? Title
) : BasicPageModel(Title)
{
    [Required(ErrorMessage = "Select if this is a provisional legislative area")]
    public bool? IsProvisionalLegislativeArea { get; init; }
    public HelperDate? AppointmentDate { get; init; }
    public HelperDate? ReviewDate { get; init; }
    public string? Reason { get; init; }
}