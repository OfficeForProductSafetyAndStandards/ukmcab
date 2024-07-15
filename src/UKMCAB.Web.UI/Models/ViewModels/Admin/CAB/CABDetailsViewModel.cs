using FluentValidation;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using UKMCAB.Core.Security;
using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{

    public class CABDetailsViewModel : CreateEditCABViewModel, ILayoutModel
    {
        public CABDetailsViewModel()
        {
            DocumentStatus = Status.Draft;
        }

        public CABDetailsViewModel(Document document, ClaimsPrincipal user, bool fromSummary = false, string? returnUrl = null)
        {
            CABId = document.CABId;
            Name = document.Name;
            CABNumber = document.CABNumber;
            PreviousCABNumbers = document.PreviousCABNumbers;
            CabNumberVisibility = document.CabNumberVisibility;
            AppointmentDateDay = document.AppointmentDate?.Day.ToString("00") ?? string.Empty;
            AppointmentDateMonth = document.AppointmentDate?.Month.ToString("00") ?? string.Empty;
            AppointmentDateYear = document.AppointmentDate?.Year.ToString("0000") ?? string.Empty;
            ReviewDateDay = document.RenewalDate?.Day.ToString("00") ?? string.Empty;
            ReviewDateMonth = document.RenewalDate?.Month.ToString("00") ?? string.Empty;
            ReviewDateYear = document.RenewalDate?.Year.ToString("0000") ?? string.Empty;
            UKASReference = document.UKASReference;
            DocumentStatus = document.StatusValue;

            IsFromSummary = fromSummary;
            ReturnUrl = returnUrl;
            IsOPSSUser = user.IsInRole(Roles.OPSS.Id);
            IsCabNumberDisabled = !IsOPSSUser;
        }

        public string? CABId { get; set; }
        public string? Name { get; set; }

        [RegularExpression(@"^[\w\d\s(),-]*$", ErrorMessage = "Enter a CAB number using only numbers and letters")]
        public string? CABNumber { get; set; }
        public string? PreviousCABNumbers { get; set; }
        public string? AppointmentDateDay { get; set; }
        public string? AppointmentDateMonth { get; set; }
        public string? AppointmentDateYear { get; set; }
        public string AppointmentDate => $"{AppointmentDateDay}/{AppointmentDateMonth}/{AppointmentDateYear}";

        public string? ReviewDateDay { get; set; }
        public string? ReviewDateMonth { get; set; }
        public string? ReviewDateYear { get; set; }
        public string ReviewDate => $"{ReviewDateDay}/{ReviewDateMonth}/{ReviewDateYear}";

        [RegularExpression(@"^\d*$", ErrorMessage = "Enter a UKAS reference number using only numbers")]
        public string? UKASReference { get; set; }
        public string? Title => $"{(!IsNew ? "Edit" : "Create")} a CAB";
        public string? CabNumberVisibility { get; set; }
        public string? SubmitType { get; set; }
        public bool IsNew { get; set; }
        public bool IsCabNumberDisabled { get; set; }
        public bool IsCabNumberVisible { get; set; }
        public bool IsOPSSUser { get; set; }
    }

    public class CABDetailsViewModelValidator : AbstractValidator<CABDetailsViewModel>
    {
        public CABDetailsViewModelValidator(IHttpContextAccessor httpContextAccessor)
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Name).NotEmpty().WithMessage("Enter a CAB name");
        }
    }
}
