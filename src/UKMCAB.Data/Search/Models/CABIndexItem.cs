using Azure.Search.Documents.Indexes;

namespace UKMCAB.Data.Search.Models
{
    public class CABIndexItem
    {
        [SimpleField(IsKey = true)] public string Id { get; set; } = string.Empty;

        [SearchableField(IsFacetable = true, IsFilterable = true)]
        public string StatusValue { get; set; } = string.Empty;

        [SimpleField] public string Status { get; set; } = string.Empty;

        [SearchableField(IsFacetable = true, IsFilterable = true)]
        public string SubStatus { get; set; } = string.Empty;

        [SearchableField(IsFacetable = true, IsFilterable = true)]
        public string LastUserGroup { get; set; } = string.Empty;

        [SimpleField] public string CABId { get; set; } = string.Empty;


        [SearchableField(IsSortable = true, NormalizerName = "lowercase", AnalyzerName = "en.microsoft")]
        public string? Name { get; set; }

        [SimpleField] public string URLSlug { get; set; } = string.Empty;
        [SearchableField]
        public string? AddressLine1 { get; set; }
        [SearchableField]
        public string? AddressLine2 { get; set; }
        [SearchableField]
        public string? TownCity { get; set; }

        [SearchableField] public string County { get; set; } = string.Empty;
        [SearchableField]
        public string? Postcode { get; set; }
        [SearchableField]
        public string? Country { get; set; }

        [SearchableField]
        public string? Email { get; set; }

        [SearchableField]
        public string? Website { get; set; }

        [SearchableField]
        public string? Phone { get; set; }

        [SearchableField(AnalyzerName = "en.microsoft")]
        public string HiddenText { get; set; } = string.Empty;
        
        [SearchableField(AnalyzerName = "en.microsoft")]
        public string[] HiddenScopeOfAppointments { get; set; } = Array.Empty<string>();

        [SearchableField] public string CABNumber { get; set; } = string.Empty;

        [SearchableField(IsFacetable = true, IsFilterable = true)]
        public string[] BodyTypes { get; set; }= Array.Empty<string>();

        [SearchableField(IsFacetable = true, IsFilterable = true, AnalyzerName = "en.microsoft")]
        public string[] LegislativeAreas { get; set; } = Array.Empty<string>();

        [SearchableField] public string[] TestingLocations { get; set; } = Array.Empty<string>();

        [SearchableField(IsFacetable = true, IsFilterable = true)]
        public string? RegisteredOfficeLocation { get; set; }

        [SimpleField(IsSortable = true)]
        public DateTime? LastUpdatedDate { get; set; }
        [SearchableField]
        public string? ScheduleLabels { get; set; }

        [SearchableField] public string? DocumentLabels { get; set; }


        [SimpleField(IsSortable = true)]
        public string RandomSort { get; set; } = string.Empty;

        [SearchableField(IsFacetable = true, IsFilterable = true)]
        public string CreatedByUserGroup { get; set; } = string.Empty;

        [SearchableField]
        public string? UKASReference { get; set; }

        [SearchableField] public List<DocumentLegislativeAreaIndexItem> DocumentLegislativeAreas { get; set; } = new();
    }
}
