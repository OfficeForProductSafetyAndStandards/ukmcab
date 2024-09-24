using System.ComponentModel.DataAnnotations;
using UKMCAB.Data.Models;
using static UKMCAB.Data.DataConstants;

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
        }

        public string? CABId { get; set; }

        [Required(ErrorMessage = "Enter an address")]
        public string? AddressLine1 { get; set; }

        public string? AddressLine2 { get; set; }

        [Required(ErrorMessage = "Enter a town or city")]
        public string? TownCity { get; set; }

        [Required(ErrorMessage = "Enter a postcode")]
        [MaxLength(20, ErrorMessage = "Maximum postcode length is 20 characters")]
        [RegularExpression(@"^[a-zA-Z0-9().\-\ ]+$", ErrorMessage = "Enter a postcode in the correct format")]
        public string? Postcode { get; set; }

        public string? County { get; set; }

        [Required(ErrorMessage = "Select a country")]
        public string? Country { get; set; }

        [RegularExpression(@"^http(s)?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$", ErrorMessage = "Enter a URL in the correct format")]
        public string? Website { get; set; }

        [RegularExpression("^([a-zA-Z0-9._%-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,})$", ErrorMessage = "Enter an email address in the correct format, like name@example.com")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Enter a telephone number")]
        [MaxLength(20, ErrorMessage = "Maximum telephone number length is 20 characters")]
        [RegularExpression(@"^((\+\d{1,3})|0)[\d\s()-]{9,}$", ErrorMessage = "Enter a telephone number, like 01632 960 001, 07700 900 982 or +44 808 157 0192")]
        public string? Phone { get; set; }

        public string? PointOfContactName { get; set; }

        [RegularExpression("^([a-zA-Z0-9._%-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,})$", ErrorMessage = "Enter an email address in the correct format, like name@example.com")]
        public string? PointOfContactEmail { get; set; }

        [MaxLength(20, ErrorMessage = "Maximum telephone number length is 20 characters")]
        [RegularExpression(@"^((\+\d{1,3})|0)[\d\s()-]{9,}$", ErrorMessage = "Enter a telephone number, like 01632 960 001, 07700 900 982 or +44 808 157 0192")]
        public string? PointOfContactPhone { get; set; }       

        public bool? IsPointOfContactPublicDisplay { get; set; }

        [Required(ErrorMessage = "Select a registered office location")]
        public string? RegisteredOfficeLocation { get; set; }

        public string? Title => "Contact details";

        public string[] FieldOrder => new[] { nameof(AddressLine1), nameof(TownCity), nameof(Postcode), nameof(Country), nameof(Website), nameof(Email), nameof(Phone), 
            nameof(PointOfContactEmail), nameof(PointOfContactPhone), nameof(IsPointOfContactPublicDisplay), nameof(RegisteredOfficeLocation) };
    }
}
