using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class SubCategoryViewModel : LegislativeAreaBaseViewModel
    {
        [Required(ErrorMessage = "Select a product sub-category")]
        public Guid? SelectedSubCategoryId { get; set; }

        public ListItem? LegislativeArea { get; set; }

        public ListItem? PurposeOfAppointment { get; set; }

        public ListItem? Category { get; set; }

        public IEnumerable<SelectListItem>? SubCategories { get; set; }
    }
}
