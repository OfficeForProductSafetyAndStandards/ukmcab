using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class CategoryViewModel : LegislativeAreaBaseViewModel
    {
        [Required(ErrorMessage = "Select a product category")]
        public Guid? SelectedCategoryId { get; set; }

        public string? LegislativeArea { get; set; }

        public string? PurposeOfAppointment { get; set; }

        public IEnumerable<SelectListItem>? Categories { get; set; }
    }
}
