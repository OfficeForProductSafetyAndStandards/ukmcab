using System.Text.Json.Serialization;

namespace UKMCAB.Core.Models.Legacy
{
    public class CABDocument
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        public string Name { get; set; }
        public string Address { get; set; }
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

        [JsonPropertyName("HiddenText")]
        public string LegacyHiddenText { get; set; }
        public string LegacyUrl { get; set; }
        [JsonPropertyName("Metadata")]
        public Dictionary<string, string> LegacyMetaData { get; set; }
    }
}
