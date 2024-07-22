using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{
    public class CABSummaryViewModel : ILayoutModel
    {
        public string? Id { get; set; }
        public Status Status { get; set; }
        public string? StatusCssStyle { get; set; }
        public SubStatus SubStatus { get; set; }
        public string? SubStatusName { get; set; }
        public string? CABId { get; set; }
        public CABDetailsViewModel? CabDetailsViewModel { get; set; }
        public CABContactViewModel? CabContactViewModel { get; set; }
        public CABBodyDetailsViewModel? CabBodyDetailsViewModel { get; set; }
        public CABLegislativeAreasViewModel? CabLegislativeAreasViewModel { get; set; }
        public CABProductScheduleDetailsViewModel? CABProductScheduleDetailsViewModel { get; set; }
        public CABSupportingDocumentDetailsViewModel? CABSupportingDocumentDetailsViewModel { get; set; }
        public CABHistoryViewModel? CABHistoryViewModel { get; set; }
        public CABGovernmentUserNotesViewModel? CABGovernmentUserNotesViewModel { get; set; }
        public string TitleHint { get; set; } = "CAB profile";
        public string? ReturnUrl { get; set; }
        public bool ShowSubstatusName => !string.IsNullOrWhiteSpace(SubStatusName) && SubStatus != SubStatus.None;
        public bool IsPendingOgdApproval { get; set; }
        public bool IsOPSSOrInCreatorUserGroup { get; set; }
        public bool IsEditLocked { get; set; }
        public bool RevealEditActions { get; set; }
        public DateTime? PublishedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string? SuccessBannerMessage { get; set; }
        public int LegislativeAreasPendingApprovalCount { get; set; }
        public bool IsOpssAdmin { get; set; }
        public bool IsUkas { get; set; }
        public bool UserInCreatorUserGroup { get; set; }
        public bool HasOgdRole { get; set; }
        public int LegislativeAreasApprovedByAdminCount { get; set; }
        public bool LegislativeAreaHasBeenActioned { get; set; }
        public bool HasActionableLegislativeAreaForOpssAdmin { get; set; }
        public bool RequestedFromCabProfilePage { get; set; }
        public bool HasActiveLAs { get; set; }
        public bool DraftUpdated { get; set; }
        public bool ShowProfileVisibilityWarning => ValidCAB && CanPublish && ShowSubSectionEditAction;
        public bool ShowMandatoryInfoWarning => !ValidCAB && CanPublish && SubStatus == SubStatus.None;
        public bool ShowReviewButton => SubStatus != SubStatus.None && (
        ShowOgdActions || (CanPublish && LegislativeAreasPendingApprovalCount > 0 && ShowSubSectionEditAction));
        public bool ShowPublishButton => SubStatus == SubStatus.None && ShowSubSectionEditAction && CanPublish;
        public bool ShowApproveToPublishButton => (CanPublish && SubStatus == SubStatus.PendingApprovalToPublish && LegislativeAreasApprovedByAdminCount > 0) && ShowSubSectionEditAction;
        public bool ShowDeclineButton =>  SubStatus != SubStatus.None && ShowSubSectionEditAction &&
            (CanPublish && SubStatus == SubStatus.PendingApprovalToPublish && LegislativeAreasApprovedByAdminCount > 0);
        public bool ShowSubmitForApprovalButton => SubStatus == SubStatus.None && ShowSubSectionEditAction && CanSubmitForApproval;
        public bool ShowSaveAsDraftButton => SubStatus == SubStatus.None && ShowSubSectionEditAction;
        public bool ShowDeleteDraftButton => SubStatus == SubStatus.None && (
            (ShowSubSectionEditAction || ShowOpssDeleteDraftActionOnly) &&
            CabDetailsViewModel.DocumentStatus == Status.Draft
        );
        public bool ShowCancelPublishButton => SubStatus == SubStatus.None && (ShowSubSectionEditAction || ShowOpssDeleteDraftActionOnly) && CabDetailsViewModel.DocumentStatus == Status.Draft;

        public bool ShowEditButton => !RevealEditActions  &&
        (SubStatus == SubStatus.None && IsEditLocked == false && (IsOpssAdmin || IsUkas) ||
        (SubStatus == SubStatus.PendingApprovalToPublish && !IsEditLocked && IsPendingOgdApproval && IsMatchingOgdUser) ||
        (SubStatus == SubStatus.PendingApprovalToPublish && !IsEditLocked && HasActionableLegislativeAreaForOpssAdmin && IsOpssAdmin) ||
        (IsOpssAdmin && UserInCreatorUserGroup)); // Replaced CanPublish with UserInCreatorUserGroup

        public bool CanOnlyBeActionedByUkas =>
            CabLegislativeAreasViewModel != null &&
            CabLegislativeAreasViewModel.ActiveLegislativeAreas.Any(la => la.Status != LAStatus.Published) &&
            CabLegislativeAreasViewModel.ActiveLegislativeAreas.Where(la => la.Status != LAStatus.Published).All(
                la => la.CanOnlyBeActionedByUkas);

        public string Title => GetTitle();

        public string GetTitle() => IsOpssAdmin
            ? SubStatus == SubStatus.PendingApprovalToPublish
                ? "Check details before approving or declining"
                : "Check details before publishing"
            : UserInCreatorUserGroup ? "Check details before submitting for approval" : "Summary";

        public bool IsMatchingOgdUser => LegislativeAreasPendingApprovalCount > 0;

        public bool ShowOgdActions =>
            HasOgdRole &&
            IsPendingOgdApproval &&
            RevealEditActions &&
            !IsEditLocked &&
            LegislativeAreasPendingApprovalCount > 0;

        public bool CanPublish => 
            IsOpssAdmin &&
            DraftUpdated && 
            !IsPendingOgdApproval && 
            !CanOnlyBeActionedByUkas;

        public bool ShowSubSectionEditAction =>
            RevealEditActions &&
            !IsEditLocked && (
                (SubStatus != SubStatus.PendingApprovalToPublish && UserInCreatorUserGroup) ||
                (SubStatus == SubStatus.PendingApprovalToPublish && IsOpssAdmin && 
                (HasActionableLegislativeAreaForOpssAdmin || CanPublish)));

        public bool ShowOpssDeleteDraftActionOnly =>
            RevealEditActions && SubStatus != SubStatus.PendingApprovalToPublish && IsOpssAdmin;

        public bool EditByGroupPermitted =>
            SubStatus != SubStatus.PendingApprovalToPublish &&
            (Status == Status.Published || UserInCreatorUserGroup);

        public bool ValidCAB =>
            Status != Status.Published &&
            IsComplete &&
            HasActiveLAs;

        public bool IsComplete =>
            CabDetailsViewModel != null &&
            CabDetailsViewModel.IsCompleted &&
            CabBodyDetailsViewModel != null &&
            CabBodyDetailsViewModel.IsCompleted &&
            CabContactViewModel != null &&
            CabContactViewModel.IsCompleted &&
            CabLegislativeAreasViewModel != null &&
            CabLegislativeAreasViewModel.IsCompleted &&
            CABProductScheduleDetailsViewModel != null &&
            CABProductScheduleDetailsViewModel.IsCompleted &&
            CABSupportingDocumentDetailsViewModel != null &&
            CABSupportingDocumentDetailsViewModel.IsCompleted;

        public bool CanSubmitForApproval => IsUkas && DraftUpdated && IsComplete;

        public string BannerContent => GetBannerContent();

        public string GetBannerContent()
        {
            if (CanPublish || (SubStatus == SubStatus.PendingApprovalToPublish && HasActionableLegislativeAreaForOpssAdmin && IsOpssAdmin))
            {
                return string.Empty;
            }
            if (SubStatus == SubStatus.PendingApprovalToPublish && (!IsPendingOgdApproval || !IsMatchingOgdUser))
            {
                return "This CAB profile cannot be edited until it's been approved or declined.";
            }
            else if (EditByGroupPermitted == false && Status == Status.Draft && IsPendingOgdApproval == false)
            {
                if (IsOpssAdmin || !IsUkas)
                {
                    if (RequestedFromCabProfilePage)
                    {
                        return "This CAB profile cannot be edited as a draft CAB profile has already been created by a UKAS user.";
                    }
                    else
                    {
                        return "This CAB profile cannot be edited as it was created by a UKAS user.";
                    }
                }
                else
                {
                    return "This CAB profile cannot be edited as a draft CAB profile has already been created by an OPSS user.";
                }
            }
            else if (EditByGroupPermitted == false && IsPendingOgdApproval == false)
            {
                return "This CAB profile cannot be edited as it was created by an OPSS user.";
            }
            else if (IsEditLocked == true && IsPendingOgdApproval == false)
            {
                return "This CAB profile cannot be edited as it's being edited by another user.";
            }

            return string.Empty;
        }
    }
}
