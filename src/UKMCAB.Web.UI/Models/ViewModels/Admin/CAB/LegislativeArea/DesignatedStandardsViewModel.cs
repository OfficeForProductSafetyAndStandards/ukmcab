using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using UKMCAB.Data.Models.LegislativeAreas;
using UKMCAB.Data.Pagination;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class DesignatedStandardsViewModel : LegislativeAreaBaseViewModel
    {
        public Guid? CompareScopeId { get; set; }
        public string? LegislativeArea { get; set; }
        [Required, MinLength(1, ErrorMessage = "Select a designated standard")]
        public List<Guid> SelectedDesignatedStandardIds { get; set; } = new();
        public List<Guid> PageSelectedDesignatedStandardIds { get; set; } = new();
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; }

        public SelectListItem SelectAll { get; set; } = new SelectListItem("Select all", Guid.Empty.ToString());
        public IEnumerable<DesignatedStandardViewModel> DesignatedStandardViewModels { get; set; } = Enumerable.Empty<DesignatedStandardViewModel>();
        public PaginationInfo? PaginationInfo { get; set; }

        public DesignatedStandardsViewModel() : base("Legislative area designated standards") 
        { 
        }

        public DesignatedStandardsViewModel(
            Guid cabId, 
            Guid scopeId, 
            bool isFromSummary, 
            Guid? compareScopeId, 
            string? legislativeAreaName, 
            List<Guid> selectedDesignatedStandardIds,
            IEnumerable<DesignatedStandardViewModel> designatedStandardViewModels,
            PaginationInfo? paginationInfo) : base("Select designated standard", cabId, scopeId, isFromSummary)
        {
            CompareScopeId = compareScopeId;
            LegislativeArea = legislativeAreaName;
            SelectedDesignatedStandardIds = selectedDesignatedStandardIds;
            DesignatedStandardViewModels = designatedStandardViewModels;
            PaginationInfo = paginationInfo;
            PageNumber = paginationInfo.PageIndex + 1;
        }
    }
}
