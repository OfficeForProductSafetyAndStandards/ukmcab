namespace UKMCAB.Core.Models
{
    public class Document
    {
        public string id { get; set; }

        public bool IsPublished { get; set; }
        public bool IsLatest { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public DateTime LastUpdatedDate => LastModifiedDate;
        public string PublishedBy { get; set; }
        public DateTime PublishedDate { get; set; }

        public CABData CABData { get; set; } // TODO: needs to go

        // Create/Details
        public string CABId { get; set; }
        public string Name { get; set; }
        public string CABNumber { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public DateTime? RenewalDate { get; set; }
        public string UKASReference { get; set; }

        // Contact
        public string Address { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string TownCity { get; set; }
        public string Postcode { get; set; }
        public string Country { get; set; }
        public string Website { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string PointOfContactName { get; set; }
        public string PointOfContactEmail { get; set; }
        public string PointOfContactPhone { get; set; }
        public bool IsPointOfContactPublicDisplay { get; set; }
        public string RegisteredOfficeLocation { get; set; }


        public List<string> TestingLocations { get; set; }
        public List<string> BodyTypes { get; set; }
        public List<string> LegislativeAreas { get; set; }
        public List<FileUpload>? Schedules { get; set; }
        public List<FileUpload>? Documents { get; set; }
        public string HiddenText { get; set; }
        public string RandomSort { get; set; }
    }
}
