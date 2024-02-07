using System.ComponentModel;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Shared;
using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;

public record LegislativeAreaAdditionalInformationViewModel(
    string? Title
) : BasicPageModel(Title)
{
    [Required(ErrorMessage = "Select if this is a provisional legislative area")]
    public bool? IsProvisionalLegislativeArea { get; set; }

    // public string? AppointmentDateDay { get; set; }
    // public string? AppointmentDateMonth { get; set; }
    // public string? AppointmentDateYear { get; set; }
    //public string AppointmentDate => $"{AppointmentDateDay}/{AppointmentDateMonth}/{AppointmentDateYear}";

    //  public  DateTime? AppointmentDate1 { get; set; }
    [DisplayName("Appointment date")] 
    public DateTime? AppointmentDate { get; set; }
    [DisplayName("Review date")]
    public DateTime? ReviewDate { get; set; }

    // public string? ReviewDateMonth { get; set; }
    // public string? ReviewDateYear { get; set; }
    // public string ReviewDate => $"{ReviewDateDay}/{ReviewDateMonth}/{ReviewDateYear}";
    public string? Reason { get; set; }
}