using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;

public record LegislativeAreaAdditionalInformationViewModel(
    string? Title
) : BasicPageModel(Title)
{
    public Guid? CabId { get; set; }
    public Guid? LegislativeAreaId { get; set; }

    [Required(ErrorMessage = "Select if this is a provisional legislative area")]
    public bool? IsProvisionalLegislativeArea { get; init; }
    [DisplayName("Appointment date")] 
    public DateTime? AppointmentDate { get; init; }
    [DisplayName("Review date")]
    public DateTime? ReviewDate { get; set; }
    public string? Reason { get; init; }
}