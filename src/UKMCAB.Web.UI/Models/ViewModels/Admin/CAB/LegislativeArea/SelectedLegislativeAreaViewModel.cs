using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class SelectedLegislativeAreaViewModel
    {   
        public string? Id { get; set; }
        public string? LegislativeAreaName { get; set; }
        public List<LegislativeAreaDetails>? LegislativeAreaDetails { get; set; }      

    }
}
