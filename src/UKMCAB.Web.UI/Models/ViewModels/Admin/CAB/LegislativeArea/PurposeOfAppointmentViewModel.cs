using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class PurposeOfAppointmentViewModel : LegislativeAreaBaseViewModel
    {
        [Required(ErrorMessage = "Select a purpose of appointment")]
        public Guid? SelectedPurposeOfAppointmentId { get; set; }

        public ListItem? LegislativeArea { get; set; }

        public IEnumerable<SelectListItem>? PurposeOfAppointments { get; set; }
    }
}
