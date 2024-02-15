using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class LegislativeAreaListItemViewModel : LegislativeAreaBaseViewModel
    { 
        public ListItem? LegislativeArea { get; set; }

        public string? PurposeOfAppointment { get; set; }

        public string? Category { get; set; }

        public string? SubCategory { get; set; }

        public List<string>? Products { get; set; } = new();

        public List<string>? Procedures { get; set; } = new();

    }
}
