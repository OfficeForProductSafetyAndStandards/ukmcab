namespace UKMCAB.Core.Domain.LegislativeAreas
{
    public class DesignatedStandardModel
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public Guid LegislativeAreaId { get; private set; }
        public List<string> ReferenceNumber { get; private set; }
        public string NoticeOfPublicationReference { get; private set; }

        public DesignatedStandardModel(Guid id, string name, Guid legislativeAreaId, List<string> referenceNumber, string noticeOfPublicationReference)
        {
            Id = id;
            Name = name;
            LegislativeAreaId = legislativeAreaId;
            ReferenceNumber = referenceNumber;
            NoticeOfPublicationReference = noticeOfPublicationReference;
        }
    }
}
