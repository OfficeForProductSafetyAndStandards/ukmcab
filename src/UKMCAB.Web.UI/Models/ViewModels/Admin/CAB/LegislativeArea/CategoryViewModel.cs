using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class CategoryViewModel : LegislativeAreaBaseViewModel
    {
        public CategoryViewModel()
        {
            Title = "Legislative area category";
        }

        [Required(ErrorMessage = "Select a product category")]
        public Guid? SelectedCategoryId { get; set; }

        [Required(ErrorMessage = "Select a category")]
        public List<Guid>? SelectedCategoryIds { get; set; }

        public string? LegislativeArea { get; set; }

        public string? PurposeOfAppointment { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
        public bool HasProducts { get; set; }
    }
}
