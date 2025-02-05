using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class AreaOfCompetencyViewModel : LegislativeAreaBaseViewModel
    {        
        [Required(ErrorMessage = "Select an area of competency")]
        public List<Guid>? SelectedAreaOfCompetencyIds { get; set; }
        public string? LegislativeArea { get; set; }
        public string? PpeCategory { get; set; }
        public string? PpeProductType { get; set; }
        public string? ProtectionAgainstRisk { get; set; }
        public IEnumerable<SelectListItem> AreaOfCompetencies { get; set; } = new List<SelectListItem>();
        public AreaOfCompetencyViewModel() : this("Create a CAB") { }
        public AreaOfCompetencyViewModel(string subTitle) : base("Legislative area - area of competencies", subTitle) { }
    }
}
