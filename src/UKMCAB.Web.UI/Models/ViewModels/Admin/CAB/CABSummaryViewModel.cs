using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{
    public class CABSummaryViewModel: ILayoutModel
    {
        public string? Id { get; set; }
        public Status Status { get; set; }
        public string? StatusCssStyle { get; init; }
        public SubStatus SubStatus { get; set; }
        public string? SubStatusName { get; set; }
        public string? CABId { get; set; }
        public CABDetailsViewModel? CabDetailsViewModel { get; set; }
        public CABContactViewModel? CabContactViewModel { get; set; }
        public CABBodyDetailsViewModel? CabBodyDetailsViewModel { get; set; }
        public List<FileUpload>? Schedules { get; set; }
        public List<FileUpload>? Documents { get; set; }
        public bool ValidCAB { get; set; }
        public string Title { get; set; } = string.Empty;
        public string TitleHint { get; set; } = string.Empty;
        public string? ReturnUrl { get; set; }
        public bool CABNameAlreadyExists { get; set; }
        public bool CanPublish { get; set; }
        public bool CanSubmitForApproval { get; set; }
        public bool ShowEditActions { get; set; }
        public bool IsOPSSOrInCreatorUserGroup { get; set; }
        public bool IsEditLocked { get; set; }
        public bool EditByGroupPermitted { get; set; }
        public bool SubSectionEditAllowed { get; set; }
        public DateTime? PublishedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public int GovernmentUserNoteCount { get; set; }
        public DateTime? LastGovermentUserNoteDate { get; set; }
    }
}
