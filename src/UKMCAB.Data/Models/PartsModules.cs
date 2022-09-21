using System.Text.Json.Serialization;

namespace UKMCAB.Data.Models;

public class PartsModules
{
    [JsonPropertyName("label")]
    public string Label { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }
    
}