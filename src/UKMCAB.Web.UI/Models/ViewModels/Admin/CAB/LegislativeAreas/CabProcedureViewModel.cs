using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeAreas
{
    public class CabProcedureViewModel : CabLegislativeAreaBaseViewModel
    {
        [Required(ErrorMessage = "Select an applicable conformity assessment procedure")]
        public IEnumerable<string>? SelectedProcedureIds { get; set; }

        public ListItem? LegislativeArea { get; set; }

        public ListItem? PurposeOfAppointment { get; set; }

        public ListItem? Category { get; set; }

        public ListItem? SubCategory { get; set; }

        public ListItem? Product { get; set; }

        public IEnumerable<SelectListItem>? Procedures { get; set; }

    }
}
