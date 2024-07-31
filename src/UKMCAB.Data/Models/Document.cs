namespace UKMCAB.Data.Models
{
    public class Document
    {
        // ReSharper disable once InconsistentNaming
        public string id { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;

        public Status StatusValue { get; set; }
        public string Status => StatusValue.ToString();

        public SubStatus SubStatus { get; set; }
        public string SubStatusName => SubStatus.ToString();

        // New audit
        public List<Audit> AuditLog { get; set; } = new();

        // Used by the search index, saves a lot of effort to flatten the model in the data source
        public DateTime LastUpdatedDate
        {
            get;
            set;
        }

        /// <summary>
        /// Last Audit Log entry using User Role Label
        /// </summary>
        public string LastUserGroup =>
            AuditLog.Any() ? AuditLog.OrderBy(al => al.DateTime).Last().UserRole : string.Empty;
        public string CreatedByUserGroup { get; set; } = string.Empty;

        // About
        public string CABId { get; set; } = string.Empty;
        public string? Name { get; set; } = string.Empty;
        public string URLSlug { get; set; } = string.Empty;
        public string? CABNumber { get; set; }
        public string? CabNumberVisibility { get; set; }
        public string? PreviousCABNumbers { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public DateTime? RenewalDate { get; set; }
        public string? UKASReference { get; set; } = string.Empty;

        // Contact
        public string? AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string? TownCity { get; set; } = string.Empty;
        public string? County { get; set; }
        public string? Postcode { get; set; } = string.Empty;
        public string? Country { get; set; } = string.Empty;
        public string? Website { get; set; } 
        public string? Email { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string? PointOfContactName { get; set; }
        public string? PointOfContactEmail { get; set; }
        public string? PointOfContactPhone { get; set; }
        public bool? IsPointOfContactPublicDisplay { get; set; }
        public string? RegisteredOfficeLocation { get; set; }

        // Body details
        public List<string> TestingLocations { get; set; } = new();
        public List<string> BodyTypes { get; set; } = new();
        
        public List<DocumentLegislativeArea> DocumentLegislativeAreas { get; set; } = new();

        public List<DocumentScopeOfAppointment> ScopeOfAppointments { get; set; } = new();

        // Schedules of accreditation
        public List<FileUpload>? Schedules { get; set; } = new();

        // Schedules of accreditation
        public List<FileUpload> ActiveSchedules => Schedules != null && Schedules.Any() ? Schedules.Where(n => n.Archived is false or null).ToList() : new();

        public List<FileUpload> ArchivedSchedules => Schedules != null && Schedules.Any() ? Schedules.Where(n => n.Archived == true).ToList() : new();

        public string ScheduleLabels => string.Join(", ", Schedules?.Select(sch => sch.Label) ?? new List<string>());

        // Supporting documents
        public List<FileUpload>? Documents { get; set; }

        public string DocumentLabels => string.Join(", ", Documents?.Select(doc => doc.Label) ?? new List<string>());

        public List<UserNote> GovernmentUserNotes { get; set; } = new();

        public string HiddenText { get; set; } = string.Empty;
        public List<string> HiddenScopeOfAppointments { get; set; } = new(); 
        public string RandomSort { get; set; } = string.Empty;
        public string LegacyCabId { get; set; } = string.Empty;
    }
}