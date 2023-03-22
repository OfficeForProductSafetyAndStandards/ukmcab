using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin
{
    public class CABDetailsViewModel : ILayoutModel
    {
        [Required(ErrorMessage = "Enter a CAB name")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Enter a CAB number")]
        public string CABNumber { get; set; }

        public string? AppointmentDateDay { get; set; }
        public string? AppointmentDateMonth { get; set; }
        public string? AppointmentDateYear { get; set; }
        public string AppointmentDate => $"{AppointmentDateDay}/{AppointmentDateMonth}/{AppointmentDateYear}";

        public string? RenewalDateDay { get; set; }
        public string? RenewalDateMonth { get; set; }
        public string? RenewalDateYear { get; set; }
        public string RenewalDate => $"{RenewalDateDay}/{RenewalDateMonth}/{RenewalDateYear}";

        public string? UKASReference { get; set; }

        public string? Title => "Create a CAB";
    }
}
