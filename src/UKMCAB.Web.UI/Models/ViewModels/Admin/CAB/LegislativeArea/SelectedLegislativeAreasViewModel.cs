using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class SelectedLegislativeAreasViewModel
    {
        public string? CABId { get; set; }
        public string? ReturnUrl { get; set; }
        public IEnumerable<SelectedLegislativeAreaViewModel>? SelectedLegislativeAreas { get; set; }

    }
}
