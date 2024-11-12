using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class PpeProductTypeViewModel : LegislativeAreaBaseViewModel
    {        
        [Required(ErrorMessage = "Select a ppe product type")]
        public Guid? SelectedPpeProductTypeId { get; set; }
        public string? LegislativeArea { get; set; }
        public IEnumerable<SelectListItem> PpeProductTypes { get; set; } = new List<SelectListItem>();
        public PpeProductTypeViewModel() : this("Create a CAB") { }
        public PpeProductTypeViewModel(string subTitle) : base("Legislative area ppe product type", subTitle) { }
    }
}
