using UKMCAB.Data.Models;

namespace UKMCAB.Core.Extensions
{
    public static class DocumentExtensions
    {
        public static bool HasActiveLAs(this Document document) => document.DocumentLegislativeAreas.Any(la => la.IsActive());

        public static bool IsPendingOgdApproval(this Document document) =>
            document.StatusValue == Status.Draft &&
            document.SubStatus == SubStatus.PendingApprovalToPublish &&
            document.DocumentLegislativeAreas.Any(d => d.IsPendingOgdApproval());

        public static int LegislativeAreasApprovedByAdminCount(this Document document) => document.DocumentLegislativeAreas.Count(dla => dla.ApprovedByOpssAdmin());

        public static bool LegislativeAreaHasBeenActioned(this Document document) => document.DocumentLegislativeAreas.Any(dla => dla.HasBeenActioned());

        public static bool HasActionableLegislativeAreaForOpssAdmin(this Document document) => document.DocumentLegislativeAreas.Any(dla => dla.ActionableByOpssAdmin());

        public static List<DocumentLegislativeArea> GetLegislativeAreasPendingApprovalByOgd(this Document document, string roleId)
        {
            return document.DocumentLegislativeAreas.Where(dla => dla.IsPendingOgdApproval() && roleId.Equals(dla.RoleId)).ToList();
        }

        public static List<DocumentLegislativeArea> GetLegislativeAreasPendingApprovalByOpss(this Document document)
        {
            return document.DocumentLegislativeAreas.Where(dla => dla.IsPendingApprovalByOpss()).ToList();
        }
    }
}
