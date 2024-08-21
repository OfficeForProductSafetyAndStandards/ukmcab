using UKMCAB.Core.Domain.LegislativeAreas;
using UKMCAB.Web.UI.Models.ViewModels.Shared;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea

{
    public class DesignatedStandardViewModel
    {
        public DesignatedStandardViewModel(DesignatedStandardModel designatedStandardModel, bool isSelected) 
        {
            Id = designatedStandardModel.Id;
            Name = designatedStandardModel.Name;
            LegislativeAreaId = designatedStandardModel.LegislativeAreaId;
            ReferenceNumber = designatedStandardModel.ReferenceNumber;
            NoticeOfPublicationReference = designatedStandardModel.NoticeOfPublicationReference;
            IsSelected = isSelected;
        }

        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public Guid LegislativeAreaId { get; private set; }
        public List<string> ReferenceNumber { get; private set; }
        public string NoticeOfPublicationReference { get; private set; }
        public bool IsSelected { get; private set; } = false;
        public PaginationViewModel? PaginationViewModel { get; private set; }
        public int PageNumber { get; private set; }
    }
}
