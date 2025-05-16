using OpenSearch.Client;
using System.Text.Json.Serialization;

namespace UKMCAB.Data.Search.Models
{
    public class CABIndexItem
    {
        [JsonPropertyName("id")]
        [Keyword(Name = "id")]
        public string Id { get; set; } = string.Empty;

        [Keyword(Name ="statusValue")]
        public string StatusValue { get; set; } = string.Empty;

        [Keyword(Name = "status")] 
        public string Status { get; set; } = string.Empty;

        [Keyword(Name = "subStatus")]
        public string SubStatus { get; set; } = string.Empty;

        [Keyword(Name = "lastUserGroup")]
        public string LastUserGroup { get; set; } = string.Empty;

        [Keyword(Name = "cabId")]
        public string CABId { get; set; } = string.Empty;

        [Text(Name ="name")]
        public string? Name { get; set; }

        [Keyword(Name = "urlSlug")]
        public string URLSlug { get; set; } = string.Empty;

        [Text(Name = "addressLine1")]
        public string? AddressLine1 { get; set; }

        [Text(Name = "addressLine2")]
        public string? AddressLine2 { get; set; }

        [Text(Name = "townCity")]
        public string? TownCity { get; set; }

        [Text(Name = "county")] 
        public string County { get; set; } = string.Empty;

        [Text(Name = "postCode")]
        public string? Postcode { get; set; }

        [Text(Name = "country")]
        public string? Country { get; set; }

        [Text(Name = "email")]
        public string? Email { get; set; }

        [Text(Name = "website")]
        public string? Website { get; set; }

        [Text(Name = "phone")]
        public string? Phone { get; set; }

        [Text(Name = "hiddenText")]
        public string HiddenText { get; set; } = string.Empty;

        [Text(Name = "hiddenScopeOfAppointments")]
        public string[] HiddenScopeOfAppointments { get; set; } = Array.Empty<string>();

        [Keyword(Name = "cabNumber")] 
        public string CABNumber { get; set; } = string.Empty;

        [Keyword(Name = "previousCABNumbers")] 
        public string PreviousCABNumbers { get; set; } = string.Empty;

        [Keyword(Name = "bodyTypes")]
        public string[] BodyTypes { get; set; }= Array.Empty<string>();

        [Keyword(Name = "mraCountries")]
        public string[] MRACountries { get; set; } = Array.Empty<string>();

        [Text(Name = "testingLocations")] 
        public string[] TestingLocations { get; set; } = Array.Empty<string>();

        [Keyword(Name = "registeredOfficeLocation")]
        public string? RegisteredOfficeLocation { get; set; }

        [Date(Name = "lastUpdatedDate")]
        public DateTime? LastUpdatedDate { get; set; }

        [Text(Name = "scheduleLabels")]
        public string? ScheduleLabels { get; set; }

        [Text(Name = "documentLabels")] 
        public string? DocumentLabels { get; set; }

        [Keyword(Name = "randomSort")]
        public string RandomSort { get; set; } = string.Empty;

        [Keyword(Name = "createdByUserGroup")]
        public string CreatedByUserGroup { get; set; } = string.Empty;

        [Text(Name = "ukasReference")]
        public string? UKASReference { get; set; }

        [Nested(Name = "documentLegislativeAreas")] 
        public List<DocumentLegislativeAreaIndexItem> DocumentLegislativeAreas { get; set; } = new();
    }
}
