namespace UKMCAB.Data.Models
{
    public class DocumentLegislativeArea : IEquatable<DocumentLegislativeArea>
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

        public void MarkAsDraft(Document document)
        {
            if (document.StatusValue == Models.Status.Draft && document.SubStatus == SubStatus.None &&
                    (Status == LAStatus.Published || Status == LAStatus.Declined || Status == LAStatus.DeclinedByOpssAdmin))
            {
                Status = LAStatus.Draft;
            }
        }

        public override bool Equals(object? obj) => Equals(obj as DocumentLegislativeArea);

        public bool Equals(DocumentLegislativeArea other)
        {
            if (other is null) return false;

            if (ReferenceEquals(this, other)) return true;

            if (other.GetType() != GetType()) return false;

            return other is not null &&
                LegislativeAreaName == other.LegislativeAreaName &&
                LegislativeAreaId == other.LegislativeAreaId &&
                IsProvisional == other.IsProvisional &&
                ReviewDate == other.ReviewDate &&
                AppointmentDate == other.AppointmentDate &&
                UserNotes == other.UserNotes &&
                Reason == other.Reason &&
                RequestReason == other.RequestReason &&
                PublicRequestReason == other.PublicRequestReason &&
                RequestReason == other.RequestReason &&
                PointOfContactName == other.PointOfContactName &&
                PointOfContactEmail == other.PointOfContactEmail &&
                PointOfContactPhone == other.PointOfContactPhone &&
                IsPointOfContactPublicDisplay == other.IsPointOfContactPublicDisplay &&
                RoleId == other.RoleId;
        }

        public override int GetHashCode() => (
            LegislativeAreaName, LegislativeAreaId, IsProvisional, ReviewDate, AppointmentDate, UserNotes, Reason, 
            RequestReason, PublicRequestReason, RequestReason, PointOfContactName, PointOfContactEmail, 
            PointOfContactPhone, IsPointOfContactPublicDisplay, RoleId).GetHashCode();

        public static bool operator ==(DocumentLegislativeArea documentLegislativeArea, DocumentLegislativeArea other)
        {
            if (documentLegislativeArea is null)
            {
                if (other is null)
                {
                    return true;
                }
                return false;
            }
            return documentLegislativeArea.Equals(other);
        }

        public static bool operator !=(DocumentLegislativeArea documentLegislativeArea, DocumentLegislativeArea other) => !(documentLegislativeArea == other);
    }
}
