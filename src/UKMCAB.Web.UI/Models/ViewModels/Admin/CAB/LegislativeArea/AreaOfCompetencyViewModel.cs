using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class AreaOfCompetencyViewModel : LegislativeAreaBaseViewModel
    {        
        [Required(ErrorMessage = "Select an area of competency")]
        public Guid? SelectedAreaOfCompetencyId { get; set; }
        public string? LegislativeArea { get; set; }

        public IEnumerable<SelectListItem> AreaOfCompetencies { get; set; } = new List<SelectListItem>();
        public AreaOfCompetencyViewModel() : base("Legislative area - area of competencies") { }
    }
}
