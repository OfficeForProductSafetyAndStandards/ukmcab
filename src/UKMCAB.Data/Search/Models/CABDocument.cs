using Azure.Search.Documents.Indexes;

namespace UKMCAB.Data.Search.Models
{
    public class CABDocument
    {
        [SimpleField(IsKey = true)]
        public string id { get; set; }

        [SearchableField(IsSortable = true, NormalizerName = "lowercase")]
        public string Name { get; set; }

        [SearchableField]
        public string Address { get; set; }

        [SearchableField]
        public string Email { get; set; }

        [SearchableField]
        public string Website { get; set; }

        [SearchableField]
        public string Phone { get; set; }

        [SearchableField]
        public string HiddenText { get; set; }

        [SearchableField]
        public string BodyNumber { get; set; }

        [SearchableField(IsFacetable = true, IsFilterable = true)]
        public string[] BodyTypes { get; set; }

        [SearchableField(IsFacetable = true, IsFilterable = true)]
        public string[] LegislativeAreas { get; set; }

        [SearchableField(IsFacetable = true, IsFilterable = true)]
        public string[] TestingLocations { get; set; }

        [SearchableField(IsFacetable = true, IsFilterable = true)]
        public string RegisteredOfficeLocation { get; set; }

        [SimpleField(IsSortable = true)]
        public DateTime? LastUpdatedDate { get; set; }

        [SimpleField(IsSortable = true)]
        public string RandomSort { get; set; }

        [SearchableField]
        public MetaData MetaData { get; set; }
    }

    public class MetaData
    {
        [SearchableField]
        public string bodynumber { get; set; }
        [SearchableField]
        public string lastupdated { get; set; }
        [SearchableField]
        public string bodytype { get; set; }
        [SearchableField]
        public string registeredofficelocation { get; set; }
        [SearchableField]
        public string testinglocations { get; set; }
        [SearchableField]
        public string website { get; set; }
        [SearchableField]
        public string email { get; set; }
        [SearchableField]
        public string phone { get; set; }
        [SearchableField]
        public string legislativearea { get; set; }
        [SearchableField]
        public string from { get; set; }
        [SearchableField]
        public string published { get; set; }
    }
}
