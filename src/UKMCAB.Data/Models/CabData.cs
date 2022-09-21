using System.Text.Json.Serialization;

namespace UKMCAB.Data.Models;

public class CabData
{
    [JsonPropertyName("externalID")]
    public string ExternalID { get; set; }
    public string? SearchFields { get; set; }


    [JsonPropertyName("name")]
    public string Name { get; set; }
    

    [JsonPropertyName("address")]
    public string Address { get; set; }

    [JsonPropertyName("website")]
    public string Website { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("phone")]
    public string Phone { get; set; }
    

    [JsonPropertyName("regulation")]
    public List<Regulation> Regulations { get; set; }
    public List<string> RegulationNames => Regulations?.Select(x => x.RegulationName)?.Distinct()?.ToList() ?? new List<string?>();
    
    
    [JsonPropertyName("bodyNumber")]
    public string BodyNumber { get; set; }
    
    [JsonPropertyName("bodyType")]
    public List<string> BodyType { get; set; }

    [JsonPropertyName("registeredOfficeLocation")]
    public List<string> RegisteredOfficeLocation { get; set; }

    [JsonPropertyName("testingLocations")]
    public List<string> TestingLocations { get; set; }
    

    [JsonPropertyName("certificationActivitiesLocations")]
    public List<CertificationActivitiesLocation> CertificationActivitiesLocations { get; set; }
    

    [JsonPropertyName("pdfUrls")]
    public string PdfUrls { get; set; }

    [JsonPropertyName("setName")]
    public string SetName { get; set; }

    [JsonPropertyName("legacyUrl")]
    public string LegacyUrl { get; set; }

    [JsonPropertyName("pdfs")]
    public string Pdfs { get; set; }

    [JsonPropertyName("appointmentDetails")]
    public string AppointmentDetails { get; set; }


    public string? RawJsonData { get; set; }
    public string? RawAllPdfText { get; set; }
    public string? RawAllText { get; set; }
}
