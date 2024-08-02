using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class PurposeOfAppointmentViewModel : LegislativeAreaBaseViewModel
    {
        public PurposeOfAppointmentViewModel()
        {
            Title = "Legislative area purpose of appointment";
        } 

        [Required(ErrorMessage = "Select a purpose of appointment")]
        public Guid? SelectedPurposeOfAppointmentId { get; set; }
        
        public Guid CabId { get; set; }

        public string? LegislativeArea { get; set; } = string.Empty;

        public IEnumerable<SelectListItem>? PurposeOfAppointments { get; set; }
    }
}
