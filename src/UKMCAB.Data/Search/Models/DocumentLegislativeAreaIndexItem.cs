using OpenSearch.Client;

namespace UKMCAB.Data.Search.Models
{
    public class DocumentLegislativeAreaIndexItem
    {
        [Ignore]
        public Guid Id { get; set; }
        
        [Text(Name = "legislativeAreaName")]
        public string LegislativeAreaName { get; set; } = string.Empty;
        [Ignore]
        public Guid LegislativeAreaId { get; set; }

        public DateTime? AppointmentDate { get; set; }

        [Boolean(Name = "isProvisional")]
        public bool? IsProvisional { get; set; }

        public DateTime? ReviewDate { get; set; }

        public string? Reason { get; set; }

        [Boolean(Name = "archived")]
        public bool? Archived { get; set; }
        
        [Keyword(Name = "status")]
        public string? Status { get; set; }
    }
}
