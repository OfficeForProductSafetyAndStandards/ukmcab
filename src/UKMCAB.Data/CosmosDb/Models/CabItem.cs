using Newtonsoft.Json;

namespace UKMCAB.Data.CosmosDb.Models
{
    public class CabItem
    {
        [JsonProperty(PropertyName = "cab")]
        public CAB CAB { get; set; }
    }
}
