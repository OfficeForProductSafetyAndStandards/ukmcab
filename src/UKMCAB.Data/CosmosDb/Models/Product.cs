using System.Text.Json.Serialization;

namespace UKMCAB.Data.CosmosDb.Models
{
    public class Product
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("code")]
        public string? Code { get; set; }
        [JsonPropertyName("partName")]
        public string? PartName { get; set; }
        [JsonPropertyName("moduleName")]
        public string? ModuleName { get; set; }
        [JsonPropertyName("scheduleName")]
        public string? ScheduleName { get; set; }
        [JsonPropertyName("standardsNumber")]
        public string? StandardsNumber { get; set; }
    }
}
