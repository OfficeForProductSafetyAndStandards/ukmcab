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
        public bool ShowEditAction { get; set; }
        public bool ShowSubstatusName { get; set; }

        //public bool ShowEditActions { get; set; }
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
        public bool IsUkas { get; set; }
        public int LegislativeAreasApprovedByAdminCount { get; set; }
        public bool LegislativeAreaHasBeenActioned { get; set; }
        public bool HasActionableLegislativeAreaForOpssAdmin { get; set; }
        public bool CanOnlyBeActionedByUkas => CabLegislativeAreasViewModel != null &&
                                                            CabLegislativeAreasViewModel.ActiveLegislativeAreas != null &&
                                                            CabLegislativeAreasViewModel.ActiveLegislativeAreas.Any(la => la.Status != LAStatus.Published) &&
                                                            CabLegislativeAreasViewModel.ActiveLegislativeAreas.Where(la => la.Status != LAStatus.Published).All( la => 
                                                                la.Status == LAStatus.DeclinedByOpssAdmin ||
                                                                la.Status == LAStatus.DeclinedToArchiveAndArchiveScheduleByOPSS ||
                                                                la.Status == LAStatus.DeclinedToArchiveAndRemoveScheduleByOPSS ||
                                                                la.Status == LAStatus.DeclinedToRemoveByOPSS ||
                                                                la.Status == LAStatus.DeclinedToUnarchiveByOPSS 
                                                                );

        public bool LoggedInUserGroupIsOwner { get; set; }
        public bool RequestedFromCabProfilePage { get; set; }
        public string BannerContent => GetBannerContent();
        public bool HasActiveLAs { get; set; }

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
