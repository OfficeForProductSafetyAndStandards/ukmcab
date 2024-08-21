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
        public PaginationViewModel? PaginationViewModel { get; set; }
        public int PageNumber { get; set; }
    }
}
