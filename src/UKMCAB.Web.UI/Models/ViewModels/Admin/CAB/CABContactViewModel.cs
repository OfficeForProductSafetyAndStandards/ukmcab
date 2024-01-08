using System.ComponentModel.DataAnnotations;
using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{
    public class CABContactViewModel : CreateEditCABViewModel, ILayoutModel
    {

        public CABContactViewModel() { }

        public CABContactViewModel(Document document)
        {
            CABId = document.CABId;
            AddressLine1 = document.AddressLine1;
            AddressLine2 = document.AddressLine2;
            TownCity = document.TownCity;
            County = document.County;
            Postcode = document.Postcode;
            Country = document.Country;
            Website = document.Website;
            Email = document.Email;
            Phone = document.Phone;
            PointOfContactName = document.PointOfContactName;
            PointOfContactEmail = document.PointOfContactEmail;
            PointOfContactPhone = document.PointOfContactPhone;
            IsPointOfContactPublicDisplay = document.IsPointOfContactPublicDisplay;
            RegisteredOfficeLocation = document.RegisteredOfficeLocation;
            DocumentStatus = document.StatusValue;
            IsCompleted = document.AddressLine1.IsNotNullOrEmpty() && document.TownCity.IsNotNullOrEmpty() 
                            && document.Postcode.IsNotNullOrEmpty() && document.Country.IsNotNullOrEmpty() 
                            && document.Phone.IsNotNullOrEmpty() && document.RegisteredOfficeLocation.IsNotNullOrEmpty();
        }

        public string? CABId { get; set; }

        [Required(ErrorMessage = "Enter an address")]
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        [Required(ErrorMessage = "Enter a town or city")]
        public string? TownCity { get; set; }
        [Required(ErrorMessage = "Enter a postcode")]
        public string? Postcode { get; set; }
        public string? County { get; set; }
        [Required(ErrorMessage = "Enter a country")]
        public string? Country { get; set; }

        public string? FormattedAddress => string.Join("<br />",
            new [] { AddressLine1, AddressLine2, TownCity, County, Postcode, Country }.Where(a =>
                !string.IsNullOrWhiteSpace(a)));

        public string? Website { get; set; }
        public string? Email { get; set; }
        [Required(ErrorMessage = "Enter a telephone number")]
        public string? Phone { get; set; }
        public string? PointOfContactName { get; set; }
        public string? PointOfContactEmail { get; set; }
        public string? PointOfContactPhone { get; set; }
        [Required(ErrorMessage = "Select who should see the point of contact details")]
        public bool? IsPointOfContactPublicDisplay { get; set; }
        [Required(ErrorMessage = "Enter a registered office location")]
        public string? RegisteredOfficeLocation { get; set; }

        public string? Title => "Contact details";

        public string[] FieldOrder => new[] { nameof(AddressLine1), nameof(TownCity), nameof(Postcode), nameof(Country), nameof(Email), nameof(Phone), nameof(IsPointOfContactPublicDisplay), nameof(RegisteredOfficeLocation) };
    }
}
