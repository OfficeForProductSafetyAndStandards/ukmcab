using System.Text.Json.Serialization;

namespace UKMCAB.Data.Models;

public class Product
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

}