// Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
using System.Text.Json.Serialization;

public class CertificationActivitiesLocation
{
    [JsonPropertyName("key")]
    public string Key { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("ncContentTypeAlias")]
    public string NcContentTypeAlias { get; set; }

    [JsonPropertyName("PropType")]
    public object PropType { get; set; }

    [JsonPropertyName("line")]
    public string Line { get; set; }
}

public class Line
{
    [JsonPropertyName("key")]
    public string Key { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("ncContentTypeAlias")]
    public string NcContentTypeAlias { get; set; }

    [JsonPropertyName("PropType")]
    public object PropType { get; set; }

    [JsonPropertyName("line")]
    public string Text { get; set; }
}

public class ProductGroup
{
    [JsonPropertyName("schedule")]
    public List<Schedule> Schedules { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("lines")]
    public List<Line> Lines { get; set; }

    [JsonPropertyName("standardsSpecificationsList")]
    public List<StandardsSpecificationsList> StandardsSpecificationsList { get; set; }
}

public class Regulation
{
    [JsonPropertyName("productGroup")]
    public List<ProductGroup> ProductGroups { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }
}

public class CabData
{
    [JsonPropertyName("regulation")]
    public List<Regulation> Regulations { get; set; }

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

    [JsonPropertyName("bodyNumber")]
    public string BodyNumber { get; set; }

    [JsonPropertyName("accreditationBodyName")]
    public string AccreditationBodyName { get; set; }

    [JsonPropertyName("accreditationBodyAddress")]
    public string AccreditationBodyAddress { get; set; }

    [JsonPropertyName("accreditationStandard")]
    public string AccreditationStandard { get; set; }

    [JsonPropertyName("bodyType")]
    public List<string> BodyType { get; set; }

    [JsonPropertyName("registeredOfficeLocation")]
    public List<string> RegisteredOfficeLocation { get; set; }

    [JsonPropertyName("certificationActivitiesLocations")]
    public List<CertificationActivitiesLocation> CertificationActivitiesLocations { get; set; }

    [JsonPropertyName("pdfUrls")]
    public string PdfUrls { get; set; }

    [JsonPropertyName("setName")]
    public string SetName { get; set; }

    [JsonPropertyName("legacyUrl")]
    public string LegacyUrl { get; set; }

    [JsonPropertyName("externalID")]
    public string ExternalID { get; set; }

    [JsonPropertyName("pdfs")]
    public string Pdfs { get; set; }

    [JsonPropertyName("testingLocations")]
    public List<string> TestingLocations { get; set; }

    [JsonPropertyName("appointmentDetails")]
    public string AppointmentDetails { get; set; }

    [JsonPropertyName("legislativeAreas")]
    public List<string> LegislativeAreas { get; set; }

    public string? RawJsonData { get; set; }
    public string? RawAllPdfText { get; set; }
    public string? RawAllText { get; set; }
}

public class Schedule
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("label")]
    public string Label { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }
}

public class StandardsSpecificationsList
{
    [JsonPropertyName("key")]
    public string Key { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("ncContentTypeAlias")]
    public string NcContentTypeAlias { get; set; }

    [JsonPropertyName("PropType")]
    public object PropType { get; set; }

    [JsonPropertyName("label")]
    public string Label { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }
}

