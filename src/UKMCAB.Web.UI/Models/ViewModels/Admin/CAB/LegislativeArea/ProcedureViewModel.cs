using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class ProcedureViewModel : LegislativeAreaBaseViewModel
    {
        [Required(ErrorMessage = "Select an applicable conformity assessment procedure")]
        public IEnumerable<Guid>? SelectedProcedureIds { get; set; }

        public string? LegislativeArea { get; set; }

        public ListItem? PurposeOfAppointment { get; set; }

        public string? Category { get; set; }

        public string? SubCategory { get; set; }

        public string? Product { get; set; }

        public IEnumerable<SelectListItem>? Procedures { get; set; }

    }
}
