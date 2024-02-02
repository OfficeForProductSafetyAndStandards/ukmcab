using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeAreas
{
    public class CabSubProductViewModel : CabLegislativeAreaBaseViewModel
    {
        [Required(ErrorMessage = "Select a product")]
        public string? SelectedproductId { get; set; }

        public ListItem? LegislativeArea { get; set; }

        public ListItem? PurposeOfAppointment { get; set; }

        public ListItem? Category { get; set; }

        public ListItem? SubCategory { get; set; }

        public IEnumerable<SelectListItem>? Products { get; set; }
    }
}
