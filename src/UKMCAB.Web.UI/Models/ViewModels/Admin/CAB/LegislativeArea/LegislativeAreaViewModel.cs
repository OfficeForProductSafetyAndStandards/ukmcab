using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class LegislativeAreaViewModel : LegislativeAreaBaseViewModel
    {
        public Guid? CABId { get; set; }

        [Required(ErrorMessage = "Select a legislative area")]
        public Guid SelectedLegislativeAreaId { get; set; }

        public IEnumerable<SelectListItem>? LegislativeAreas { get; set; }
    }
}
