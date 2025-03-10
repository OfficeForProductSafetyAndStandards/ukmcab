﻿namespace UKMCAB.Data.Models
{
    public class DocumentLegislativeArea
    {
        public Guid Id { get; set; }

        public string LegislativeAreaName { get; set; } = string.Empty;
        public Guid LegislativeAreaId { get; set; }

        public DateTime? AppointmentDate { get; set; }

        public bool? IsProvisional { get; set; }

        public DateTime? ReviewDate { get; set; }

        public string? UserNotes { get; set; }
        public string? Reason { get; set; }

        public string? RequestReason { get; set; }
        public string? PublicRequestReason { get; set; }

        public bool? Archived { get; set; }

        public string? PointOfContactName { get; set; }
        public string? PointOfContactEmail { get; set; }
        public string? PointOfContactPhone { get; set; }
        public bool? IsPointOfContactPublicDisplay { get; set; }
        public LAStatus Status { get; set; }
        public string RoleId { get; set; }
        public bool? NewlyCreated { get; set; }   
        public bool? MRABypass { get; set; }
    }
}
