using System.Text.Json.Serialization;

namespace UKMCAB.Data.Models;

public class Regulation
{
    [JsonPropertyName("productGroup")]
    public List<ProductGroup> ProductGroups { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("regulationDescriptor")]
    public List<string> RegulationDescriptor { get; set; }

    public string RegulationName => RegulationDescriptor?.FirstOrDefault() ?? string.Empty;
}
