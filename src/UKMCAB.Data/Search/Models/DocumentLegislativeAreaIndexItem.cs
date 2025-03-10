﻿using System.Text.Json.Serialization;
using Azure.Search.Documents.Indexes;

namespace UKMCAB.Data.Search.Models
{
    public class DocumentLegislativeAreaIndexItem
    {
        [JsonIgnore]
        public Guid Id { get; set; }
        
        [SearchableField(IsFacetable = true, IsFilterable = true)]
        public string LegislativeAreaName { get; set; } = string.Empty;
        [JsonIgnore]
        public Guid LegislativeAreaId { get; set; }

        public DateTime? AppointmentDate { get; set; }

        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public bool? IsProvisional { get; set; }

        public DateTime? ReviewDate { get; set; }

        public string? Reason { get; set; }
        
        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public bool? Archived { get; set; }
        
        [SimpleField(IsFacetable = true, IsFilterable = true)]
        public string? Status { get; set; }
    }
}
