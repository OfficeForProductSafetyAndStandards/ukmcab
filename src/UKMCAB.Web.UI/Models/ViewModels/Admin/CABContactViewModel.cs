
using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin
{
    public class CABContactViewModel :ILayoutModel
    {
        public string? CABId { get; set; }

        [Required(ErrorMessage = "Enter an address")]
        public string AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        [Required(ErrorMessage = "Enter a town or city")]
        public string TownCity { get; set; }
        [Required(ErrorMessage = "Enter a postcode")]
        public string Postcode { get; set; }
        [Required(ErrorMessage = "Enter a country")]
        public string Country { get; set; }
        public string? Website { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? PointOfContactName { get; set; }
        public string? PointOfContactEmail { get; set; }
        public string? PointOfContactPhone { get; set; }
        public bool IsPointOfContactPublicDisplay { get; set; }
        [Required(ErrorMessage = "Enter a registered office location")]
        public string RegisteredOfficeLocation { get; set; }

        public List<string> CountryList => Constants.Lists.Countries;

        public string? Title => "Contact details";

        public string[] FieldOrder => new[] { nameof(AddressLine1), nameof(TownCity), nameof(Postcode), nameof(Country), nameof(Email), nameof(RegisteredOfficeLocation) };
    }
}
