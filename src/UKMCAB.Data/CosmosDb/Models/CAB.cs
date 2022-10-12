using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace UKMCAB.Data.CosmosDb.Models
{
    public class CAB
    {
        [JsonProperty(PropertyName = "id")]
        public string? Id { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string? Name { get; set; }
        [JsonProperty(PropertyName = "address")]
        public string? Address { get; set; }
        [JsonProperty(PropertyName = "email")]
        public string? Email { get; set; }
        [JsonProperty(PropertyName = "phone")]
        public string? Phone { get; set; }
        [JsonProperty(PropertyName = "website")]
        public string? Website { get; set; }
        [JsonProperty(PropertyName = "bodyNumber")]
        public string? BodyNumber { get; set; }
        [JsonProperty(PropertyName = "bodyType")]
        public string? BodyType { get; set; }
        [JsonProperty(PropertyName = "registeredOfficeLocation")]
        public string? RegisteredOfficeLocation { get; set; }
        [JsonProperty(PropertyName = "testingLocations")]
        public string? TestingLocations { get; set; }
        [JsonProperty(PropertyName = "regulations")]
        public List<Regulation> Regulations { get; set; }
    }
}
