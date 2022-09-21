using System.Text.Json.Serialization;

namespace UKMCAB.Data.Models;

public class ProductGroup
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("lines")]
    public List<Product> Products { get; set; }
    
    [JsonPropertyName("schedule")]
    public List<Schedule> Schedules { get; set; }
    
    [JsonPropertyName("standardsSpecificationsList")]
    public List<SpecificationsStandards> SpecificationsStandards { get; set; }
}
