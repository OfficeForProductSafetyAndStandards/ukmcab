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

    public string? PointOfContactName { get; set; }

    [RegularExpression("^([a-zA-Z0-9._%-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,})$", ErrorMessage = "Enter an email address in the correct format, like name@example.com")]
    public string? PointOfContactEmail { get; set; }

    [MaxLength(20, ErrorMessage = "Maximum telephone number length is 20 characters")]
    [RegularExpression(@"^((\+\d{1,3})|0)[\d\s()-]{9,}$", ErrorMessage = "Enter a telephone number, like 01632 960 001, 07700 900 982 or +44 808 157 0192")]
    public string? PointOfContactPhone { get; set; }

    public bool? IsPointOfContactPublicDisplay { get; set; }

    public bool IsFromSummary { get; set; }
    public string SubmitType { get; set; }
}