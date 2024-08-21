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
        public string? SearchTerm { get; set; }
        public string? PaginationSearchTerm { get; set; }
        public int? PageNumber { get; set; }

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
            List<Guid> pageDesignatedStandardsIds,
            IEnumerable<DesignatedStandardViewModel> designatedStandardViewModels,
            PaginationInfo? paginationInfo) : base("Select designated standard", cabId, scopeId, isFromSummary)
        {
            CompareScopeId = compareScopeId;
            LegislativeArea = legislativeAreaName;
            SelectedDesignatedStandardIds = selectedDesignatedStandardIds;
            DesignatedStandardViewModels = designatedStandardViewModels;
            PageDesignatedStandardsIds = pageDesignatedStandardsIds;
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

        public void SetDesignatedStandardViewModels(IEnumerable<DesignatedStandardModel> designatedStandards)
        {
            var designatedStandardViewModels = designatedStandards.Select(d => new DesignatedStandardViewModel(d)).ToList();
            designatedStandardViewModels.Where(d => SelectedDesignatedStandardIds.Contains(d.Id)).ForEach(d =>
            {
                d.IsSelected = true;
            });
            DesignatedStandardViewModels = designatedStandardViewModels;
        }

        public void SetPageDesignatedStandardsIds()
        {
            PageDesignatedStandardsIds = DesignatedStandardViewModels.Select(d => d.Id).ToList();
        }
    }
}
