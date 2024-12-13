using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class ProtectionAgainstRiskViewModel : LegislativeAreaBaseViewModel
    {                
        public Guid? SelectedProtectionAgainstRiskId { get; set; }
        [Required(ErrorMessage = "Select a protection against risk")]
        public List<Guid>? SelectedProtectionAgainstRiskIds { get; set; }
        public string? LegislativeArea { get; set; }
        public string? PpeCategory { get; set; }
        public string? PpeProductType { get; set; }
        public IEnumerable<SelectListItem> ProtectionAgainstRisks { get; set; } = new List<SelectListItem>();
        public ProtectionAgainstRiskViewModel() : this("Create a CAB") { }
        public ProtectionAgainstRiskViewModel(string subTitle) : base("Legislative area providing protection against the following risks", subTitle) { }
    }
}
