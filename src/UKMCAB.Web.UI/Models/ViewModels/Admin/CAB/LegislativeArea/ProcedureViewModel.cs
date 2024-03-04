using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class ProcedureViewModel : LegislativeAreaBaseViewModel
    {
        [Required(ErrorMessage = "Select an applicable conformity assessment procedure")]
        public IEnumerable<Guid>? SelectedProcedureIds { get; set; }

        public Guid? LegislativeAreaId { get; set; }

        public string? LegislativeArea { get; set; }

        public string? PurposeOfAppointment { get; set; }

        public string? Category { get; set; }

        public string? SubCategory { get; set; }

        public string? Product { get; set; }
        public Guid? CurrentProductId { get; set; }

        public IEnumerable<SelectListItem> Procedures { get; set; } = new List<SelectListItem>();

        public int IndexOfProduct { get; set; }
        public bool IsLastAction { get; set; } = true;

    }
}
