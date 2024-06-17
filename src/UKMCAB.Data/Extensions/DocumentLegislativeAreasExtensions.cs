using UKMCAB.Data.Models;

namespace UKMCAB.Data.Extensions
{
    public static class DocumentLegislativeAreasExtensions
    {
        public static bool HasAnyBeenActioned(this List<DocumentLegislativeArea> documentLegislativeAreas)
            => documentLegislativeAreas.Any(la => la.Status is
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
                LAStatus.DeclinedToUnarchiveByOPSS);
    }
}
