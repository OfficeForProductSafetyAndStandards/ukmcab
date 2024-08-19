using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using UKMCAB.Core.Domain.LegislativeAreas;
using UKMCAB.Web.UI.Models.ViewModels.Shared;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class DesignatedStandardsViewModel : LegislativeAreaBaseViewModel
    {
        [Required(ErrorMessage = "Select a designated standard")]
        public List<Guid>? SelectedDesignatedStandardIds { get; set; }
        public string? SearchTerm { get; set; }
        public Guid CabId { get; set; }
        public string? LegislativeArea { get; set; } = string.Empty;
        public Guid? LegislativeAreaId { get; set; }
        public IEnumerable<DesignatedStandardViewModel> DesignatedStandardViewModels { get; set; } = Enumerable.Empty<DesignatedStandardViewModel>();
        public SelectListItem SelectAll { get; set; } = new SelectListItem("Select all", Guid.Empty.ToString());
        public PaginationViewModel? PaginationViewModel { get; set; }
        public int PageNumber { get; set; }

        public DesignatedStandardsViewModel() : base("Legislative area designated standards") { }
    }
}
