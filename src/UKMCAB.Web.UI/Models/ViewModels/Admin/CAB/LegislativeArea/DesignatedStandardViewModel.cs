using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using UKMCAB.Core.Domain.LegislativeAreas;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class DesignatedStandardViewModel : LegislativeAreaBaseViewModel
    {
        [Required(ErrorMessage = "Select a designated standard")]
        public List<Guid>? SelectedDesignatedStandardIds { get; set; }
        public string? SearchTerm { get; set; }
        public Guid CabId { get; set; }
        public string? LegislativeArea { get; set; } = string.Empty;
        public Guid? LegislativeAreaId { get; set; }
        public IEnumerable<DesignatedStandardModel> DesignatedStandardModels { get; set; }

        public SelectListItem SelectAll { get; set; } = new SelectListItem("Select all", "SelectAll");
        public IEnumerable<SelectListItem>? DesignatedStandards { get; set; }
        public DesignatedStandardViewModel() : base("Legislative area designated standards") { }
    }
}
