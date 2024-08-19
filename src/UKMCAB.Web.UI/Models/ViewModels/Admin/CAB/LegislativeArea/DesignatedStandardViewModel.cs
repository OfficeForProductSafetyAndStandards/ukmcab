using UKMCAB.Core.Domain.LegislativeAreas;
using UKMCAB.Web.UI.Models.ViewModels.Shared;
using UKMCAB.Data.Models.LegislativeAreas;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea

{
    public class DesignatedStandardViewModel
    {
        public DesignatedStandardViewModel(DesignatedStandardModel designatedStandardModel) 
        {
            Id = designatedStandardModel.Id;
            Name = designatedStandardModel.Name;
            LegislativeAreaId = designatedStandardModel.LegislativeAreaId;
            ReferenceNumber = designatedStandardModel.ReferenceNumber;
            NoticeOfPublicationReference = designatedStandardModel.NoticeOfPublicationReference;
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid LegislativeAreaId { get; set; }
        public List<string> ReferenceNumber { get; set; }
        public string NoticeOfPublicationReference { get; set; }
        public bool IsSelected { get; set; } = false;

        //[Required(ErrorMessage = "Select a designated standard")]
        //public List<Guid>? SelectedDesignatedStandardIds { get; set; }
        //public string? SearchTerm { get; set; }
        //public Guid CabId { get; set; }
        //public string? LegislativeArea { get; set; } = string.Empty;
        //public Guid? LegislativeAreaId { get; set; }
        //public IEnumerable<DesignatedStandardModel> DesignatedStandardModels { get; set; }
        //public SelectListItem SelectAll { get; set; } = new SelectListItem("Select all", "SelectAll");
        //public IEnumerable<SelectListItem>? DesignatedStandards { get; set; }
        public PaginationViewModel? PaginationViewModel { get; set; }
        public int PageNumber { get; set; }
        //public DesignatedStandardViewModel() : base("Legislative area designated standards") { }
    }
}
