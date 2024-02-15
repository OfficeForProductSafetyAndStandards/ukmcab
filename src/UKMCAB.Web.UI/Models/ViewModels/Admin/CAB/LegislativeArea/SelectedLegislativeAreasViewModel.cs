using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class SelectedLegislativeAreasViewModel: ILayoutModel
    {
        public Guid CABId { get; set; }
        public string? ReturnUrl { get; set; }
        public IEnumerable<SelectedLegislativeAreaViewModel>? SelectedLegislativeAreas { get; set; }
        public string? Title => "Legislative areas added";
    }
}
