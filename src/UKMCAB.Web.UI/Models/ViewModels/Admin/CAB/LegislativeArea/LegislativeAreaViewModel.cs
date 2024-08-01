using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class LegislativeAreaViewModel : LegislativeAreaBaseViewModel
    {
        public LegislativeAreaViewModel()
        {
            Title = "Legislative area";
        }

        [Required(ErrorMessage = "Select a legislative area")]
        public Guid SelectedLegislativeAreaId { get; set; }

        public IEnumerable<SelectListItem>? LegislativeAreas { get; set; }
    }
}
