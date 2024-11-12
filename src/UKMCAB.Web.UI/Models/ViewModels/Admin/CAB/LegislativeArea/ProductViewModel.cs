using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class ProductViewModel : LegislativeAreaBaseViewModel
    {
        [Required(ErrorMessage = "Select a product")]
        public List<Guid>? SelectedProductIds { get; set; }

        public string? LegislativeArea { get; set; }

        public string? PurposeOfAppointment { get; set; }

        public string? Category { get; set; }

        public string? SubCategory { get; set; }

        public IEnumerable<SelectListItem> Products { get; set; } = new List<SelectListItem>();
        
        public ProductViewModel() : this("Create a CAB") { }
        public ProductViewModel(string subTitle) : base("Legislative area", subTitle) { }
    }
}
