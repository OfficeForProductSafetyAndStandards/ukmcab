using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class LegislativeAreaListItemViewModel : LegislativeAreaBaseViewModel
    { 
        public ListItem? LegislativeArea { get; set; }

        public string? PurposeOfAppointment { get; set; }

        public string? Category { get; set; }

        public string? SubCategory { get; set; }

        public string? Product { get; set; }

        public string? Procedure { get; set; }

    }
}
