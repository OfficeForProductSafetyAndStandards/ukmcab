using System.ComponentModel.DataAnnotations;
using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin
{

    public class CABDetailsViewModel : CreateEditCABViewModel, ILayoutModel
    {
        public CABDetailsViewModel()
        {
            DocumentStatus = Status.Draft;
        }

        public CABDetailsViewModel(Document document)
        {
            CABId = document.CABId;
            Name = document.Name;
            CABNumber = document.CABNumber;
            CabNumberVisibility = document.CabNumberVisibility;
            AppointmentDateDay = document.AppointmentDate?.Day.ToString("00") ?? string.Empty;
            AppointmentDateMonth = document.AppointmentDate?.Month.ToString("00") ?? string.Empty;
            AppointmentDateYear = document.AppointmentDate?.Year.ToString("0000") ?? string.Empty;
            ReviewDateDay = document.RenewalDate?.Day.ToString("00") ?? string.Empty;
            ReviewDateMonth = document.RenewalDate?.Month.ToString("00") ?? string.Empty;
            ReviewDateYear = document.RenewalDate?.Year.ToString("0000") ?? string.Empty;
            UKASReference = document.UKASReference;
            DocumentStatus = document.StatusValue;
        }

        public string? CABId { get; set; }

        [Required(ErrorMessage = "Enter a CAB name")]
        public string? Name { get; set; }
        [Required(ErrorMessage = "Enter a CAB number")]
        public string? CABNumber { get; set; }

        public string? AppointmentDateDay { get; set; }
        public string? AppointmentDateMonth { get; set; }
        public string? AppointmentDateYear { get; set; }
        public string AppointmentDate => $"{AppointmentDateDay}/{AppointmentDateMonth}/{AppointmentDateYear}";

        public string? ReviewDateDay { get; set; }
        public string? ReviewDateMonth { get; set; }
        public string? ReviewDateYear { get; set; }
        public string ReviewDate => $"{ReviewDateDay}/{ReviewDateMonth}/{ReviewDateYear}";
        public string? UKASReference { get; set; }
        public string? Title => $"{(!IsNew ? "Edit" : "Create")} a CAB";
        public string? CabNumberVisibility { get; set; }
        public bool IsNew { get; set; }
    }
}
