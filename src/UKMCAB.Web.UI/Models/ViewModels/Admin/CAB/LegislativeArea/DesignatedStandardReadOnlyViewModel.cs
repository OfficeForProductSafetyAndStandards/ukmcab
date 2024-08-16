namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class DesignatedStandardReadOnlyViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<string> ReferenceNumber { get; set; }
        public string NoticeOfPublicationReference { get; set; }

        public DesignatedStandardReadOnlyViewModel(Guid id, string name, List<string> referenceNumber, string noticeOfPublicationReference) 
        {
            Id = id;
            Name = name;
            ReferenceNumber = referenceNumber;
            NoticeOfPublicationReference = noticeOfPublicationReference;
        }
    }
}
