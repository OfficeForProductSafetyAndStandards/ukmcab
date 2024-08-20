using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using UKMCAB.Core.Domain.LegislativeAreas;
using UKMCAB.Web.UI.Models.ViewModels.Shared;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class DesignatedStandardsViewModel : LegislativeAreaBaseViewModel
    {
        public string? LegislativeArea { get; set; }
        public Guid? LegislativeAreaId { get; set; }
        [Required, MinLength(1, ErrorMessage = "Select a designated standard")]
        public List<Guid> SelectedDesignatedStandardIds { get; set; } = new();
        public string? SearchTerm { get; set; }

        public IEnumerable<DesignatedStandardViewModel> DesignatedStandardViewModels { get; set; } = Enumerable.Empty<DesignatedStandardViewModel>();
        public SelectListItem SelectAll { get; set; } = new SelectListItem("Select all", Guid.Empty.ToString());
        public PaginationViewModel? PaginationViewModel { get; set; }
        public int PageNumber { get; set; }

        public DesignatedStandardsViewModel() : base("Legislative area designated standards") 
        { 
        }

        public DesignatedStandardsViewModel(Guid cabId, Guid scopeId, bool isFromSummary) : base("Select designated standard", cabId, scopeId, isFromSummary)
        {
        }
    }
}
