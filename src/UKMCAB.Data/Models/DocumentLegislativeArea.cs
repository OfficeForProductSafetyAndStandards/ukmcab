namespace UKMCAB.Data.Models
{
    using Azure.Search.Documents.Indexes;

    public class DocumentLegislativeArea
    {
        public Guid Id { get; set; }

        public string LegislativeAreaName { get; set; } = string.Empty;
        public Guid LegislativeAreaId { get; set; }

        public DateTime? AppointmentDate { get; set; }

        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public bool? IsProvisional { get; set; }

        public DateTime? ReviewDate { get; set; }

        public string? Reason { get; set; }
        
        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public bool? Archived { get; set; }
    }
}
