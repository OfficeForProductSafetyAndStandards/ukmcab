using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace UKMCAB.Data.CosmosDb.Models
{
    public class Product
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("productCode")]
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
