using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Shared;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;

public record LegislativeAreaAdditionalInformationViewModel(
    string? Title
) : BasicPageModel(Title)
{
    public bool? IsProvisionalLegislativeArea { get; init; }
    public HelperDate? AppointmentDate { get; init; }
    public HelperDate? ReviewDate { get; init; }
    public string? Reason { get; init; }
}