using static UKMCAB.Common.Helpers.EnumerableHelper;

namespace UKMCAB.Data.Models
{
    public class Document : IEquatable<Document>
    {
        // ReSharper disable once InconsistentNaming
        public string id { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public Status StatusValue { get; set; }
        public SubStatus SubStatus { get; set; }
        // Used by the search index, saves a lot of effort to flatten the model in the data source
        public DateTime LastUpdatedDate { get; set; }
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
        public string RandomSort { get; set; } = string.Empty;
        public string LegacyCabId { get; set; } = string.Empty;
        public string HiddenText { get; set; } = string.Empty;
        public List<string> HiddenScopeOfAppointments { get; set; } = new();
        // New audit
        public List<Audit> AuditLog { get; set; } = new();
        // Body details
        public List<string> TestingLocations { get; set; } = new();
        public List<string> BodyTypes { get; set; } = new();
        public List<DocumentLegislativeArea> DocumentLegislativeAreas { get; set; } = new();
        public List<DocumentScopeOfAppointment> ScopeOfAppointments { get; set; } = new();
        // Schedules of accreditation
        public List<FileUpload> Schedules { get; set; } = new();
        // Supporting documents
        public List<FileUpload> Documents { get; set; } = new();
        public List<UserNote> GovernmentUserNotes { get; set; } = new();

        public string Status => StatusValue.ToString();

        public string SubStatusName => SubStatus.ToString();
        /// <summary>
        /// Last Audit Log entry using User Role Label
        /// </summary>
        public string LastUserGroup => AuditLog.Any() ? AuditLog.OrderBy(al => al.DateTime).Last().UserRole : string.Empty;
        public string DocumentLabels => string.Join(", ", Documents?.Select(doc => doc.Label) ?? new List<string>());

        public bool IsPendingOgdApproval =>
            StatusValue == Models.Status.Draft &&
            SubStatus == SubStatus.PendingApprovalToPublish &&
            DocumentLegislativeAreas.Any(d => 
                d.Status == LAStatus.PendingApproval ||
                d.Status ==  LAStatus.PendingApprovalToRemove ||
                d.Status ==  LAStatus.PendingApprovalToArchiveAndArchiveSchedule ||
                d.Status == LAStatus.PendingApprovalToArchiveAndRemoveSchedule ||
                d.Status == LAStatus.PendingApprovalToUnarchive);

        public bool HasActiveLAs => DocumentLegislativeAreas.Any(la => la.Status != LAStatus.DeclinedByOpssAdmin && la.Status != LAStatus.ApprovedToRemoveByOpssAdmin);

        public string ScheduleLabels => string.Join(", ", Schedules?.Select(sch => sch.Label) ?? new List<string>());

        public List<FileUpload> ActiveSchedules => Schedules != null && Schedules.Any() ? Schedules.Where(n => n.Archived is false or null).ToList() : new();

        public List<FileUpload> ArchivedSchedules => Schedules != null && Schedules.Any() ? Schedules.Where(n => n.Archived == true).ToList() : new();

        public override bool Equals(object? obj) => Equals(obj as Document);

        public bool Equals(Document other)
        {
            if (other is null) return false;

            if (ReferenceEquals(this, other)) return true;
            
            if (other.GetType() != GetType()) return false;

            return other is not null &&
                CreatedByUserGroup == other.CreatedByUserGroup &&
                Name == other.Name &&
                URLSlug == other.URLSlug &&
                CABNumber == other.CABNumber &&
                CabNumberVisibility == other.CabNumberVisibility &&
                PreviousCABNumbers == other.PreviousCABNumbers &&
                AppointmentDate == other.AppointmentDate &&
                RenewalDate == other.RenewalDate &&
                UKASReference == other.UKASReference &&
                AddressLine1 == other.AddressLine1 &&
                AddressLine2 == other.AddressLine2 &&
                TownCity == other.TownCity &&
                County == other.County &&
                Postcode == other.Postcode &&
                Country == other.Country &&
                Website == other.Website &&
                Email == other.Email &&
                Phone == other.Phone &&
                PointOfContactName == other.PointOfContactName &&
                PointOfContactEmail == other.PointOfContactEmail &&
                PointOfContactPhone == other.PointOfContactPhone &&
                IsPointOfContactPublicDisplay == other.IsPointOfContactPublicDisplay &&
                RegisteredOfficeLocation == other.RegisteredOfficeLocation &&
                RandomSort == other.RandomSort &&
                HiddenText == other.HiddenText &&
                AreListsEqual(HiddenScopeOfAppointments, other.HiddenScopeOfAppointments) &&
                AreListsEqual(TestingLocations, other.TestingLocations) &&
                AreListsEqual(BodyTypes, other.BodyTypes) &&
                AreObjectListsEqual(DocumentLegislativeAreas, other.DocumentLegislativeAreas) &&
                AreObjectListsEqual(ScopeOfAppointments, other.ScopeOfAppointments) &&
                AreObjectListsEqual(Schedules, other.Schedules) &&
                AreObjectListsEqual(Documents, other.Documents) &&
                AreObjectListsEqual(GovernmentUserNotes, other.GovernmentUserNotes);
        }

        public override int GetHashCode() => (
            CreatedByUserGroup, Name, URLSlug, CABNumber, CabNumberVisibility, PreviousCABNumbers, AppointmentDate, RenewalDate, UKASReference, 
            AddressLine1, AddressLine2, TownCity, County, Postcode, Country, Website, Email, Phone, PointOfContactName, PointOfContactEmail, 
            PointOfContactPhone, IsPointOfContactPublicDisplay, RegisteredOfficeLocation, RandomSort, HiddenText, HiddenScopeOfAppointments, 
            TestingLocations, BodyTypes, DocumentLegislativeAreas, ScopeOfAppointments, Schedules, Documents, GovernmentUserNotes).GetHashCode();

        public static bool operator ==(Document document, Document other)
        {
            if (document is null)
            {
                if (other is null)
                {
                    return true;
                }
                return false;
            }
            return document.Equals(other);
        }

        public static bool operator !=(Document document, Document other) => !(document == other);
    }
}