using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{
    public class CABSummaryViewModel: ILayoutModel
    {
        public SubStatus SubStatus { get; set; }
        public string? CABId { get; set; }
        public CABDetailsViewModel? CabDetailsViewModel { get; set; }
        public CABContactViewModel? CabContactViewModel { get; set; }
        public CABBodyDetailsViewModel? CabBodyDetailsViewModel { get; set; }
        public List<FileUpload>? Schedules { get; set; }
        public List<FileUpload>? Documents { get; set; }
        public bool ValidCAB { get; set; }
        public bool ShowError { get; set; }
        public string Title => "Check details before " + (CanPublish ? "publishing" : "submitting for approval");
        public string? ReturnUrl { get; set; }
        public bool CABNameAlreadyExists { get; set; }
        public bool CanPublish { get; set; }
        public bool CanSubmitForApproval { get; set; }
        public bool CanEdit { get; set; }
    }
}
