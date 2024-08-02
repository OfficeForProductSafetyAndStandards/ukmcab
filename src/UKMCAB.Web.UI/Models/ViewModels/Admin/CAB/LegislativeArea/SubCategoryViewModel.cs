using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class SubCategoryViewModel : LegislativeAreaBaseViewModel
    {
        public SubCategoryViewModel()
        {
            Title = "Legislative crea sub-category";
        }

        [Required(ErrorMessage = "Select a product sub-category")]
        public Guid? SelectedSubCategoryId { get; set; }

        public string? LegislativeArea { get; set; }

        public string? PurposeOfAppointment { get; set; }

        public string? Category { get; set; }

        public IEnumerable<SelectListItem> SubCategories { get; set; } = new List<SelectListItem>();
    }
}
