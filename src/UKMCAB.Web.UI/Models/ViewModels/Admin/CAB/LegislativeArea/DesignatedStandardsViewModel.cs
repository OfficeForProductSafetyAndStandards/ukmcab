using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using UKMCAB.Core.Domain.LegislativeAreas;
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
        public List<Guid> PageDesignatedStandardsIds { get; set; } = new();
        public List<DesignatedStandardViewModel> PageDesignatedStandardViewModels { get; set; } = new();
        public SelectListItem SelectAll { get; set; } = new SelectListItem("Select all", Guid.Empty.ToString());
        public string? SearchTerm { get; set; }
        public string? PaginationSearchTerm { get; set; }
        public int? PageNumber { get; set; } = 1;
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
            IEnumerable<DesignatedStandardModel> designatedStandards,
            PaginationInfo? paginationInfo) : base("Select designated standard", cabId, scopeId, isFromSummary)
        {
            CompareScopeId = compareScopeId;
            LegislativeArea = legislativeAreaName;
            SelectedDesignatedStandardIds = selectedDesignatedStandardIds;
            SetPageDesignatedStandardViewModels(designatedStandards);
            PaginationInfo = paginationInfo;
            PageNumber = paginationInfo is not null ? paginationInfo.PageIndex + 1 : null;
        }

        public void UpdateSelectedDesignatedStandardIds()
        {
            var newPageSelectedDesignatedStandardIds = PageSelectedDesignatedStandardIds.Except(SelectedDesignatedStandardIds);
            SelectedDesignatedStandardIds.AddRange(newPageSelectedDesignatedStandardIds);

            var removedPageDesignatedStandardIds = PageDesignatedStandardsIds.Except(PageSelectedDesignatedStandardIds);
            SelectedDesignatedStandardIds.RemoveAll(id => removedPageDesignatedStandardIds.Contains(id));
        }

        public void SetPageDesignatedStandardViewModels(IEnumerable<DesignatedStandardModel> designatedStandards)
        {
            PageDesignatedStandardViewModels = designatedStandards.Select(d => 
                new DesignatedStandardViewModel(d, SelectedDesignatedStandardIds.Contains(d.Id))).ToList();
            PageDesignatedStandardsIds = PageDesignatedStandardViewModels.Select(d => d.Id).ToList();
        }
    }
}
