using UKMCAB.Core.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.FindACAB
{
    public class CABProfileViewModel : ILayoutModel
    {
        // ILayoutModel
        public string? Title => $"CAB profile - {Name}";

        public string CABId { get; set; }

        // Publish attributes
        public DateTime PublishedDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        // About
        public string Name { get; set; }

        public string UKASReferenceNumber { get; set; }

        // Contact details
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string TownCity { get; set; }
        public string Postcode { get; set; }
        public string Country { get; set; }

        public string Address
        {
            get
            {
                var addressProperties =
                    new[] { AddressLine1, AddressLine2, TownCity, Postcode, Country }.Where(p =>
                        !string.IsNullOrWhiteSpace(p));
                return string.Join("<br />", addressProperties);
            }
        }
        public string Website { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public List<string> RegisteredOfficeLocations { get; set; }
        // Body details
        public List<string> RegisteredTestLocations { get; set; }
        public string BodyNumber { get; set; }
        public List<string> BodyTypes { get; set; }
        public List<string> LegislativeBodies { get; set; }
        public List<FileUpload> ProductSchedules { get; set; }
    }
}
