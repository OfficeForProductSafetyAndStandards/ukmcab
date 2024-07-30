using UKMCAB.Data.Models;

namespace UKMCAB.Core.Extensions
{
    public static class DocumentLegislativeAreaExtensions
    {
        public static void MarkAsDraft(this DocumentLegislativeArea documentLegislativeArea, Status documentStatusValue, SubStatus documentSubStatus)
        {
            if (documentStatusValue == Status.Draft && 
                documentSubStatus == SubStatus.None &&
                (documentLegislativeArea.Status == LAStatus.Published || 
                    documentLegislativeArea.Status == LAStatus.Declined || 
                    documentLegislativeArea.Status == LAStatus.DeclinedByOpssAdmin))
            {
                documentLegislativeArea.Status = LAStatus.Draft;
            }
        }

        public static bool ApprovedByOpssAdmin(this DocumentLegislativeArea documentLegislativeArea) => documentLegislativeArea.Status is
            LAStatus.Published or
            LAStatus.ApprovedByOpssAdmin or
            LAStatus.ApprovedToRemoveByOpssAdmin or
            LAStatus.ApprovedToArchiveAndArchiveScheduleByOpssAdmin or
            LAStatus.ApprovedToArchiveAndRemoveScheduleByOpssAdmin or
            LAStatus.ApprovedToUnarchiveByOPSS;

        public static bool IsPublished(this DocumentLegislativeArea documentLegislativeArea) => documentLegislativeArea.Status is
            LAStatus.Published;

        public static bool IsPendingOgdApproval(this DocumentLegislativeArea documentLegislativeArea) => documentLegislativeArea.Status is
            LAStatus.PendingApproval or
            LAStatus.PendingApprovalToRemove or
            LAStatus.PendingApprovalToArchiveAndArchiveSchedule or
            LAStatus.PendingApprovalToArchiveAndRemoveSchedule or
            LAStatus.PendingApprovalToUnarchive;
        
        public static bool IsPendingApprovalByOpss(this DocumentLegislativeArea documentLegislativeArea) => documentLegislativeArea.Status is
            LAStatus.Approved or
            LAStatus.PendingApprovalToRemoveByOpssAdmin or
            LAStatus.PendingApprovalToArchiveAndArchiveScheduleByOpssAdmin or
            LAStatus.PendingApprovalToArchiveAndRemoveScheduleByOpssAdmin or
            LAStatus.PendingApprovalToUnarchive or
            LAStatus.PendingApprovalToUnarchiveByOpssAdmin;

        public static bool IsActive(this DocumentLegislativeArea documentLegislativeArea) => 
            documentLegislativeArea.Status != LAStatus.DeclinedByOpssAdmin && 
            documentLegislativeArea.Status != LAStatus.ApprovedToRemoveByOpssAdmin;

        public static bool HasBeenActioned(this DocumentLegislativeArea documentLegislativeArea) => 
            documentLegislativeArea.Status is LAStatus.Published || documentLegislativeArea.ActionableByOpssAdmin();

        public static bool ActionableByOpssAdmin(this DocumentLegislativeArea documentLegislativeArea) => documentLegislativeArea.Status is
            LAStatus.Approved or
            LAStatus.Declined or
            LAStatus.DeclinedToRemoveByOPSS or
            LAStatus.ApprovedByOpssAdmin or
            LAStatus.DeclinedByOpssAdmin or
            LAStatus.PendingApprovalToRemoveByOpssAdmin or
            LAStatus.ApprovedToRemoveByOpssAdmin or
            LAStatus.ApprovedToArchiveAndArchiveScheduleByOpssAdmin or
            LAStatus.ApprovedToArchiveAndRemoveScheduleByOpssAdmin or
            LAStatus.PendingApprovalToArchiveAndArchiveScheduleByOpssAdmin or
            LAStatus.PendingApprovalToArchiveAndRemoveScheduleByOpssAdmin or
            LAStatus.DeclinedToArchiveAndArchiveScheduleByOGD or
            LAStatus.DeclinedToArchiveAndArchiveScheduleByOPSS or
            LAStatus.DeclinedToArchiveAndRemoveScheduleByOGD or
            LAStatus.DeclinedToArchiveAndRemoveScheduleByOPSS or
            LAStatus.ApprovedToUnarchiveByOPSS or
            LAStatus.PendingApprovalToUnarchiveByOpssAdmin or
            LAStatus.DeclinedToUnarchiveByOPSS;
    }
}
