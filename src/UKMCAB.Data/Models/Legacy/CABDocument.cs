using System.Text.Json.Serialization;


namespace UKMCAB.Data.Models.Legacy
{
    public class CABDocument
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        public string Name { get; set; }

        // public string Address { get; set; } // deprecated in favour of AddressLine1, AddressLine2, TownCity etc.
        public string Email { get; set; }
        public string Website { get; set; }
        public string Phone { get; set; }
        public string BodyNumber { get; set; }
        public List<string> BodyTypes { get; set; }
        public string RegisteredOfficeLocation { get; set; }
        public List<string> TestingLocations { get; set; }
        public List<string> LegislativeAreas { get; set; }

        public DateTimeOffset? PublishedDate { get; set; }
        public DateTimeOffset? LastUpdatedDate { get; set; }

        public List<PDF> PDFs { get; set; } = new List<PDF>();

        public string HiddenText { get; set; }

        public string LegacyUrl { get; set; }
        
        [JsonPropertyName("Metadata")]
        public Dictionary<string, string> LegacyMetaData { get; set; }

        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? TownCity { get; set; }
        public string? Postcode { get; set; }
        public string? County { get; set; }
        public string? Country { get; set; }
    }
}
