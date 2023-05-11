namespace UKMCAB.Data.Models
{
    public class Document
    {
        public string id { get; set; }

        public Status StatusValue { get; set; }
        public string Status => StatusValue.ToString();

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public DateTime LastUpdatedDate => LastModifiedDate;
        public string PublishedBy { get; set; }
        public DateTime PublishedDate { get; set; }
        public string ArchivedBy { get; set; }
        public DateTime ArchivedDate { get; set; }
        public string ArchivedReason { get; set; }


        // Create/Details
        public string CABId { get; set; }
        public string Name { get; set; }
        public string CABNumber { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public DateTime? RenewalDate { get; set; }
        public string UKASReference { get; set; }

        // Contact
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? TownCity { get; set; }
        public string? Postcode { get; set; }
        public string Country { get; set; }
        public string Website { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string PointOfContactName { get; set; }
        public string PointOfContactEmail { get; set; }
        public string PointOfContactPhone { get; set; }
        public bool IsPointOfContactPublicDisplay { get; set; }
        public string RegisteredOfficeLocation { get; set; }

        // Body details
        public List<string> TestingLocations { get; set; }
        public List<string> BodyTypes { get; set; }
        public List<string> LegislativeAreas { get; set; }

        // Schedules of accreditation
        public List<FileUpload>? Schedules { get; set; }

        // Supporting documents
        public List<FileUpload>? Documents { get; set; }


        public string HiddenText { get; set; }
        public string RandomSort { get; set; }
    }
}
