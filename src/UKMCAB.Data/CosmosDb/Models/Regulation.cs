using System.Text.Json.Serialization;

namespace UKMCAB.Data.CosmosDb.Models
{
    public class Regulation
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("products")]
        public List<Product> Products { get; set; }
    }
}
