using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Search
{
    public class CABProfileViewModel : ILayoutModel
    {
        // ILayoutModel
        public string? Title => $"CAB profile - {Name}";
        public string? ReturnUrl { get; set; }

        public bool IsLoggedIn { get; set; }

        public string CABId { get; set; }

        // Publish attributes
        public DateTime? PublishedDate { get; set; }

        public DateTime? LastModifiedDate { get; set; }

        // About
        public string Name { get; set; }

        public string UKASReferenceNumber { get; set; }

        // Contact details
        public string Address { get; set; }
        public string Website { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public string RegisteredOfficeLocation { get; set; }
        // Body details
        public List<string> RegisteredTestLocations { get; set; }
        public string BodyNumber { get; set; }
        public List<string> BodyTypes { get; set; }
        public List<string> LegislativeAreas { get; set; }
        public List<FileUpload> ProductSchedules { get; set; }
    }
}
