using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class LegislativeAreaListItemViewModel : LegislativeAreaBaseViewModel
    {
        [Required(ErrorMessage = "Select an applicable conformity assessment procedure")]
        public IEnumerable<string>? SelectedProcedureIds { get; set; }

        public ListItem? LegislativeArea { get; set; }

        public string? PurposeOfAppointment { get; set; }

        public string? Category { get; set; }

        public string? SubCategory { get; set; }

        public string? Product { get; set; }

    }
}
