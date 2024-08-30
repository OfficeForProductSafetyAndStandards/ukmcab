using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class ProtectionAgainstRiskViewModel : LegislativeAreaBaseViewModel
    {        
        [Required(ErrorMessage = "Select a protection against risk")]
        public Guid? SelectedProtectionAgainstRiskId { get; set; }
        public string? LegislativeArea { get; set; }
        public IEnumerable<SelectListItem> ProtectionAgainstRisks { get; set; } = new List<SelectListItem>();
        public ProtectionAgainstRiskViewModel() : base("Legislative area providing protection against the following risks") { }
    }
}
