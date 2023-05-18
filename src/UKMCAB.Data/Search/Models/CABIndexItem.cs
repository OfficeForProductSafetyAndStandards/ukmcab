using Azure.Search.Documents.Indexes;

namespace UKMCAB.Data.Search.Models
{
    public class CABIndexItem
    {
        [SimpleField(IsKey = true)]
        public string id { get; set; }
        [SimpleField]
        public string CABId { get; set; }


        [SearchableField(IsSortable = true, NormalizerName = "lowercase", AnalyzerName = "en.microsoft")]
        public string Name { get; set; }

        [SearchableField]
        public string AddressLine1 { get; set; }
        [SearchableField]
        public string AddressLine2 { get; set; }
        [SearchableField]
        public string TownCity { get; set; }
        [SearchableField]
        public string County { get; set; }
        [SearchableField]
        public string Postcode { get; set; }
        [SearchableField]
        public string Country { get; set; }

        [SearchableField]
        public string Email { get; set; }

        [SearchableField]
        public string Website { get; set; }

        [SearchableField]
        public string Phone { get; set; }

        [SearchableField(AnalyzerName = "en.microsoft")]
        public string HiddenText { get; set; }

        [SearchableField]
        public string CABNumber { get; set; }

        [SearchableField(IsFacetable = true, IsFilterable = true)]
        public string[] BodyTypes { get; set; }

        [SearchableField(IsFacetable = true, IsFilterable = true)]
        public string[] LegislativeAreas { get; set; }

        [SearchableField]
        public string[] TestingLocations { get; set; }

        [SearchableField(IsFacetable = true, IsFilterable = true)]
        public string RegisteredOfficeLocation { get; set; }

        [SimpleField(IsSortable = true)]
        public DateTime? LastUpdatedDate { get; set; }


        [SimpleField(IsSortable = true)]
        public string RandomSort { get; set; }
    }
}
