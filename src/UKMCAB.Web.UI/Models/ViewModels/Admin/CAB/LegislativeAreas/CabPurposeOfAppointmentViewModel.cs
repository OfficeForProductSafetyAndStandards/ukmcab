using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeAreas
{
    public class CabPurposeOfAppointmentViewModel : CabLegislativeAreaBaseViewModel
    {
        [Required(ErrorMessage = "Select a purpose of appointment")]
        public string? SelectedPurposeOfAppointmentId { get; set; }

        public ListItem? LegislativeArea { get; set; }

        public IEnumerable<SelectListItem>? PurposeOfAppointments { get; set; }
    }
}
