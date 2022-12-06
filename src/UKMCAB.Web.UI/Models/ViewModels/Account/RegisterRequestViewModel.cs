using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Account
{
    public class RegisterRequestViewModel: ILayoutModel, IValidatableObject
    {
        public string? Title => "Register OGD user";
        public List<string>? LegislativeAreaList { get; set; }

        public string? Role { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Legislative areas")]
        public List<string>? LegislativeAreas { get; set; }

        [Display(Name = "Reason for request")]
        public string? RequestReason { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrWhiteSpace(Email))
            {
                if (Role.Equals(Constants.Roles.UKASUser, StringComparison.InvariantCultureIgnoreCase) && !Email.EndsWith("@ukas.com", StringComparison.InvariantCultureIgnoreCase))
                {
                    yield return new ValidationResult("Only ukas.com email addresses can register for an UKAS user account.", new[] {nameof(Email)});
                }
                if (!Role.Equals(Constants.Roles.UKASUser, StringComparison.InvariantCultureIgnoreCase) && !Email.EndsWith(".gov.uk", StringComparison.InvariantCultureIgnoreCase))
                {
                    yield return new ValidationResult(Role.Equals(Constants.Roles.OGDUser) ? "Only GOV UK email addresses can register for an OGD user account." : "Only GOV UK email addresses can register for an OPSS admin account.", new[] {nameof(Email)});
                }
            }
        }
    }
}
