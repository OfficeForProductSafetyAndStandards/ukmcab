﻿using UKMCAB.Data.Models;

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

        public static bool IsActionableByOpssAdmin(this Document document) => HasActionableLegislativeAreaForOpssAdmin(document) || document.DocumentLegislativeAreas.All(dla => dla.IsPublished());

        public static List<DocumentLegislativeArea> LegislativeAreasPendingApprovalByOgd(this Document document, string roleId)
        {
            return document.DocumentLegislativeAreas.Where(dla => dla.IsPendingOgdApproval() && roleId.Equals(dla.RoleId)).ToList();
        }

        public static bool HasActionableLegislativeAreaForOgd(this Document document, string roleId)
        {
            return LegislativeAreasPendingApprovalByOgd(document, roleId).Count > 0;
        }

        public static List<DocumentLegislativeArea> LegislativeAreasPendingApprovalByOpss(this Document document)
        {
            return document.DocumentLegislativeAreas.Where(dla => dla.IsPendingApprovalByOpss()).ToList();
        }

        public static DateTime? LastGovernmentUserNoteDate(this Document document)
        {
            return Enumerable.MaxBy(document.GovernmentUserNotes, u => u.DateTime)?.DateTime;
        }

        public static DateTime? LastAuditLogHistoryDate(this Document document)
        {
            return Enumerable.MaxBy(document.AuditLog, u => u.DateTime)?.DateTime;
        }

        public static bool DraftUpdated(this Document document)
        {
            return Enumerable.MaxBy(
                document.AuditLog.Where(l => l.Action == AuditCABActions.Created),
                u => u.DateTime)?.DateTime != document.LastUpdatedDate;
        }

        public static DateTime? PublishedDate(this Document document)
        {
            return document.AuditLog.OrderBy(a => a.DateTime).FirstOrDefault(al => al.Action == AuditCABActions.Published)?.DateTime;
        }

        public static List<FileUpload> PublicDocuments(this Document document) =>
            document?.Documents?.Where(d => d.IsPublic).ToList() ?? new List<FileUpload>();

        public static bool HasPublicDocuments(this Document document) =>
            document?.Documents?.Any(c => c.IsPublic) ?? false;
    }
}
