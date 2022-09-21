using System.Text.Json.Serialization;

namespace UKMCAB.Data.Models;

public class CertificationActivitiesLocation
{
    [JsonPropertyName("line")]
    public string Location { get; set; }
}