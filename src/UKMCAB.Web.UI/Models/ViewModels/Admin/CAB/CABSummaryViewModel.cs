using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{
    public class CABSummaryViewModel : ILayoutModel
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
        public CABLegislativeAreasViewModel? CabLegislativeAreasViewModel { get; set; }
        public CABProductScheduleDetailsViewModel? CABProductScheduleDetailsViewModel { get; set; }
        public CABSupportingDocumentDetailsViewModel? CABSupportingDocumentDetailsViewModel { get; set; }
        public bool ValidCAB { get; set; }
        public string Title { get; set; } = string.Empty;
        public string TitleHint { get; set; } = string.Empty;
        public string? ReturnUrl { get; set; }
        public bool CABNameAlreadyExists { get; set; }
        public bool CanPublish { get; set; }
        public bool CanSubmitForApproval { get; set; }
        public bool ShowEditActions { get; set; }
        public bool ShowOpssDeleteDraftActionOnly { get; set; }
        public bool IsPendingOgdApproval { get; set; }
        public bool IsMatchingOgdUser { get; set; }
        public bool ShowOgdActions { get; set; }
        public bool IsOPSSOrInCreatorUserGroup { get; set; }
        public bool IsEditLocked { get; set; }
        public bool EditByGroupPermitted { get; set; }
        public bool SubSectionEditAllowed { get; set; }
        public DateTime? PublishedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public int GovernmentUserNoteCount { get; set; }
        public DateTime? LastGovernmentUserNoteDate { get; set; }
        public DateTime? LastAuditLogHistoryDate { get; set; }
        public string? SuccessBannerMessage { get; set; }
        public int LegislativeAreasPendingApprovalCount { get; set; }
        public bool IsOpssAdmin { get; set; }
        public int LegislativeAreasApprovedByAdminCount { get; set; }
        public bool LegislativeAreaHasBeenActioned { get; set; }

        public bool LoggedInUserGroupIsOwner { get; set; }
    }
}
