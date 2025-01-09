using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class PpeCategoriesViewModel : LegislativeAreaBaseViewModel
    {        
        [Required(ErrorMessage = "Select a ppe category")]
        public Guid? SelectedPpeCategoryId { get; set; }
        public string? LegislativeArea { get; set; }
        public IEnumerable<SelectListItem> PpeCategories { get; set; } = [];
        public PpeCategoriesViewModel() : this("Create a CAB") { }
        public PpeCategoriesViewModel(string subTitle) : base("Legislative area ppe category", subTitle) { }
    }
}
