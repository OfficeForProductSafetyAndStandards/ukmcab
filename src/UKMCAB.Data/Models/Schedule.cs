using System.Text.Json.Serialization;

namespace UKMCAB.Data.Models;

public class Schedule
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("label")]
    public string Label { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }
    
    [JsonPropertyName("partsModules")]
    public List<PartsModules> PartsModules { get; set; }
}
