using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeAreas
{
    public class CabLegislativeAreaViewModel : CabLegislativeAreaBaseViewModel
    {
        [Required(ErrorMessage = "Select a legislative area")]
        public string? SelectedLegislativeAreaId { get; set; }

        public IEnumerable<SelectListItem>? LegislativeAreas { get; set; }
    }
}
