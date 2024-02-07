namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;

public record LegislativeAreaDetailViewModel(
    string? Title
) : BasicPageModel(Title)
{
    public bool? IsProvisionalLegislativeArea { get; init; }
    public string? AppointmentDateDay { get; init; }
    public string? AppointmentDateMonth { get; init; }
    public string? AppointmentDateYear { get; init; }
    public string AppointmentDate => $"{AppointmentDateDay}/{AppointmentDateMonth}/{AppointmentDateYear}";
    public string? ReviewDateDay { get; init; }
    public string? ReviewDateMonth { get; init; }
    public string? ReviewDateYear { get; init; }
    public string ReviewDate => $"{ReviewDateDay}/{ReviewDateMonth}/{ReviewDateYear}";
    public string? Reason { get; init; }

    public ListItem? LegislativeArea { get; set; }
}