using Ganss.Xss;
using System.Text;
using System.Web;
using UKMCAB.Common;
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Data.Models
{
    public class Audit
    {
        public Audit()
        {
        }

        public Audit(string userId, string username, string userrole, DateTime date, string action,
            string? comment = null, string? publicComment = null, bool isUserInputComment = true,
            bool isUserEnteredPublicComment = true)
        {
            UserId = userId;
            UserName = username;
            UserRole = userrole;
            DateTime = date;
            Action = action;
            Comment = comment;
            PublicComment = publicComment;
            IsUserInputComment = isUserInputComment;
            IsUserEnteredPublicComment = isUserEnteredPublicComment;
        }

        public Audit(UserAccount? userAccount, string action, string? comment = null, string? publicComment = null,
            bool isUserInputComment = true, bool isUserEnteredPublicComment = true) : this(userAccount?.Id,
            $"{userAccount?.FirstName} {userAccount?.Surname}", userAccount?.Role, DateTime.UtcNow, action, comment,
            publicComment, isUserInputComment, isUserEnteredPublicComment)
        {
        }

        public Audit(UserAccount? userAccount, string action, Document publishedDocument,
            Document? previousDocument = null, string? comment = null, string? publicComment = null) : this(
            userAccount?.Id, $"{userAccount?.FirstName} {userAccount?.Surname}", userAccount?.Role, DateTime.UtcNow,
            action, comment, publicComment)
        {
            var sbComment = new StringBuilder();
            var sbPublicComment = new StringBuilder();

            HtmlSanitizer htmlSanitizer = new HtmlSanitizer();
            htmlSanitizer.AllowedTags.Clear();

            if (!string.IsNullOrWhiteSpace(comment))
            {
                comment = htmlSanitizer.Sanitize(comment);
            }

            if (!string.IsNullOrWhiteSpace(publicComment))
            {
                publicComment = htmlSanitizer.Sanitize(publicComment);
            }

            if (previousDocument == null)
            {
                sbComment.AppendFormat("<p class=\"govuk-body\">Appointment date: {0}</p>",
                    publishedDocument.AppointmentDate.HasValue
                        ? publishedDocument.AppointmentDate.Value.ToString("dd/MM/yyyy") + " 12:00"
                        : "Not provided");
                sbComment.AppendFormat("<p class=\"govuk-body\">Publication date: {0} 12:00</p>",
                    DateTime.UtcNow.ToString("dd/MM/yyyy"));
            }
            else
            {
                string[] schedulesAndDocsType = { "Documents", "Schedules" };

                foreach (var docType in schedulesAndDocsType)
                {
                    if (docType.Equals("Documents"))
                    {
                        var previousFileUploads = previousDocument.Documents ?? new List<FileUpload>();
                        var currentFileUploads = publishedDocument.Documents ?? new List<FileUpload>();
                        CalculateChangesToScheduleOrDocument(publishedDocument, previousDocument, sbComment,
                            previousFileUploads, currentFileUploads, docType);
                    }
                    else
                    {
                        var previousFileUploads = previousDocument.Schedules ?? new List<FileUpload>();
                        var currentFileUploads = publishedDocument.Schedules ?? new List<FileUpload>();
                        CalculateChangesToScheduleOrDocument(publishedDocument, previousDocument, sbComment,
                            previousFileUploads, currentFileUploads, docType);
                    }
                }

                CalculateChangesToLegislativeAreas(previousDocument.DocumentLegislativeAreas,
                    publishedDocument.DocumentLegislativeAreas, sbComment);

                CalculateChangesToScopeOfAppointments(previousDocument, publishedDocument, sbComment);
            }

            if (sbComment.Length > 0)
            {
                sbComment.Insert(0, "<p class=\"govuk-body\">Changes:</p>");
                if (string.IsNullOrWhiteSpace(comment))
                    IsUserInputComment = false;
            }

            if (sbPublicComment.Length > 0)
            {
                sbPublicComment.Insert(0, "<p class=\"govuk-body\">Changes:</p>");
                if (string.IsNullOrWhiteSpace(publicComment))
                    IsUserEnteredPublicComment = false;
            }

            comment = !string.IsNullOrEmpty(comment) ? $"<p class=\"govuk-body\">{comment}</p>" : string.Empty;
            publicComment = !string.IsNullOrEmpty(publicComment)
                ? $"<p class=\"govuk-body\">{publicComment}</p>"
                : string.Empty;

            Comment = string.Join("", HttpUtility.HtmlEncode(comment), HttpUtility.HtmlEncode(sbComment.ToString()));
            PublicComment = string.Join("", HttpUtility.HtmlEncode(publicComment),
                HttpUtility.HtmlEncode(sbPublicComment.ToString()));
        }

        private static void CalculateChangesToScopeOfAppointments(Document previousDocument, Document publishedDocument,
            StringBuilder sb)
        {
            var legislativeAreasRemained =
                publishedDocument.DocumentLegislativeAreas.Where(l => previousDocument.DocumentLegislativeAreas.Select(p => p.LegislativeAreaId).Contains(l.LegislativeAreaId)).ToDictionary(l => l.LegislativeAreaId,
                    l => l.LegislativeAreaName);
            
            foreach (var la in legislativeAreasRemained)
            { 
                var previousScopes = previousDocument.ScopeOfAppointments.Where(sa => sa.LegislativeAreaId == la.Key).Select(s => s.Id).ToList();
                var currentScopes = publishedDocument.ScopeOfAppointments.Where(sa => sa.LegislativeAreaId == la.Key).Select(s => s.Id).ToList();
                var countScopesRemoved =
                    previousScopes.Count(ps => !currentScopes.Contains(ps));
                var countScopesAdded =
                    currentScopes.Count(cs => !previousScopes.Contains(cs));
                if (countScopesAdded > 0)
                {
                    sb.Append(
                        $"<p class=\"govuk-body\">{countScopesAdded} scope of appointment{(countScopesAdded > 1 ? "s" : null)} {(legislativeAreasRemained.Count > 1 ? "have" : "has")} been added to {la.Value}.</p>");
                }

                if (countScopesRemoved > 0)
                {
                    sb.Append(
                        $"<p class=\"govuk-body\">{countScopesRemoved} scope of appointment{(countScopesRemoved > 1 ? "s" : null)} {(legislativeAreasRemained.Count > 1 ? "have" : "has")} been removed from {la.Value}.</p>");
                }
            }
        }

        private static void CalculateChangesToLegislativeAreas(List<DocumentLegislativeArea> previousLAs,
            List<DocumentLegislativeArea> currentLAs, StringBuilder sb)
        {
            var newlyAddedLAs =
                currentLAs.Where(la => previousLAs.All(pla => pla.LegislativeAreaId != la.LegislativeAreaId));
            var removedLAs =
                previousLAs.Where(la => currentLAs.All(cla => cla.LegislativeAreaId != la.LegislativeAreaId));

            foreach (var la in newlyAddedLAs)
            {
                sb.AppendFormat(
                    "<p class=\"govuk-body\">{0} was added to legislative area.</p>",
                    la.LegislativeAreaName);
            }

            foreach (var la in removedLAs)
            {
                sb.AppendFormat(
                    "<p class=\"govuk-body\">{0} was removed from legislative area.</p>",
                    la.LegislativeAreaName);
            }

            foreach (var currentLa in currentLAs)
            {
                var previousLa = previousLAs.FirstOrDefault(x => x.Id == currentLa.Id);

                if (previousLa != null)
                {
                    if (previousLa.PointOfContactName != currentLa.PointOfContactName ||
                        previousLa.PointOfContactEmail != currentLa.PointOfContactEmail ||
                        previousLa.PointOfContactPhone != currentLa.PointOfContactPhone)
                    {
                        sb.AppendFormat(
                            "<p class=\"govuk-body\">{0} contact details changed.</p>",
                            currentLa.LegislativeAreaName);
                    }
                }
            }
        }

        private static void CalculateChangesToScheduleOrDocument(Document publishedDocument, Document? previousDocument,
            StringBuilder sb, List<FileUpload> previousFileUploads, List<FileUpload> currentFileUploads, string docType)
        {
            var isSchedule = docType.Equals("Schedules");
            var docTypes = isSchedule ? "product schedules" : "supporting documents";
            var docTypeName = isSchedule ? "product schedule" : "supporting document";

            var existingFileUploads = currentFileUploads
                .Where(sch => previousFileUploads.Any(prev => prev.UploadDateTime.Equals(sch.UploadDateTime)));
            var newFileUploads = currentFileUploads.Where(sch =>
                previousFileUploads.All(prev => !prev.UploadDateTime.Equals(sch.UploadDateTime)));
            var removedFileUploads = previousFileUploads.Where(sch =>
                currentFileUploads.All(pub => !pub.UploadDateTime.Equals(sch.UploadDateTime)));

            foreach (var fileupload in existingFileUploads)
            {
                var previousFileUpload =
                    previousFileUploads.Single(sch => sch.UploadDateTime.Equals(fileupload.UploadDateTime));
                if (isSchedule)
                {
                    if (!previousFileUpload.LegislativeArea.Equals(fileupload.LegislativeArea))
                    {
                        sb.AppendFormat(
                            "<p class=\"govuk-body\">The legislative area {0} has been changed to {1} on this product schedule <a href=\"{2}\" target=\"_blank\" class=\"govuk-link\">{3}</a>.</p>",
                            previousFileUpload.LegislativeArea, fileupload.LegislativeArea,
                            ScheduleOrDocumentLink(publishedDocument.CABId, fileupload.FileName, docType),
                            fileupload.Label);
                    }
                }
                else
                {
                    if (!previousFileUpload.Category.Equals(fileupload.Category))
                    {
                        sb.AppendFormat(
                            "<p class=\"govuk-body\">The category {0} has been changed to {1} on this supporting document <a href=\"{2}\" target=\"_blank\" class=\"govuk-link\">{3}</a>.</p>",
                            previousFileUpload.Category, fileupload.Category,
                            ScheduleOrDocumentLink(publishedDocument.CABId, fileupload.FileName, docType),
                            fileupload.Label);
                    }
                }

                if (!previousFileUpload.Label.Equals(fileupload.Label))
                {
                    sb.AppendFormat(
                        "<p class=\"govuk-body\">The title of this {0} <a href=\"{1}\" target=\"_blank\" class=\"govuk-link\">{2}</a> has been changed from {3} to {4}.</p>",
                        docTypeName, ScheduleOrDocumentLink(publishedDocument.CABId, fileupload.FileName, docType),
                        fileupload.Label, previousFileUpload.Label, fileupload.Label);
                }
            }

            foreach (var newFileUpload in newFileUploads)
            {
                sb.AppendFormat(
                    "<p class=\"govuk-body\"><a href=\"{0}\" target=\"_blank\" class=\"govuk-link\">{1}</a> was added to the {2} page.</p>",
                    ScheduleOrDocumentLink(publishedDocument.CABId, newFileUpload.FileName, docType),
                    newFileUpload.Label, docTypes);
            }

            foreach (var removedFileUpload in removedFileUploads)
            {
                sb.AppendFormat(
                    "<p class=\"govuk-body\"><a href=\"{0}\" target=\"_blank\" class=\"govuk-link\">{1}</a> was removed from the {2} page.</p>",
                    ScheduleOrDocumentLink(publishedDocument.CABId, removedFileUpload.FileName, docType),
                    removedFileUpload.Label, docTypes);
            }
        }

        private static string ScheduleOrDocumentLink(string cabId, string fileName, string docType)
        {
            if (docType.Equals("Schedules"))
            {
                return $"/search/cab-schedule-view/{cabId}?file={fileName}&filetype=schedules";
            }

            return $"/search/cab-schedule-view/{cabId}?file={fileName}&filetype=documents";
        }

        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserRole { get; set; }
        public DateTime DateTime { get; set; }
        public string Action { get; set; }
        public string? Comment { get; set; }
        public string? PublicComment { get; set; }
        public bool? IsUserInputComment { get; set; }
        public bool? IsUserEnteredPublicComment { get; set; }
    }

    public class AuditCABActions
    {
        public const string Created = nameof(Created);
        public const string Published = nameof(Published);
        public const string RePublished = nameof(RePublished);
        public const string Archived = nameof(Archived);
        public const string Unarchived = nameof(Unarchived); // Used on a draft that has been unarchived

        public const string
            UnarchivedToDraft =
                "Unarchived to draft"; //Used for CAB's that have been unarchived (previous and new draft version)

        public const string UnarchiveApprovalRequest = "Request to unarchive"; // Ukas request for unarchive
        public const string SubmittedForApproval = "Submitted for OPSS approval"; // UKAS request to publish
        public const string CABApproved = "CAB Approved";
        public const string CABDeclined = "CAB Declined";
        public const string DraftDeleted = "Draft deleted";
        public const string ArchiveApprovalRequest = "Request to archive"; // UKAS request to archive

        public const string
            UnArchiveApprovalRequestDeclined = "CAB unarchive declined"; // Ukas request for unarchive declined

        public const string
            UnpublishApprovalRequest = "Request to unpublish"; // UKAS request to un publish and create draft

        public const string
            UnpublishApprovalRequestDeclined =
                "Request to unpublish declined"; // UKAS request to un publish and create draft

        public const string DeclineLegislativeArea = "Legislative area declined";
    }

    public class AuditUserActions
    {
        public const string UserAccountRequest = nameof(UserAccountRequest);
        public const string ApproveAccountRequest = nameof(ApproveAccountRequest);
        public const string DeclineAccountRequest = nameof(DeclineAccountRequest);
        public const string LockAccountRequest = nameof(LockAccountRequest);
        public const string UnlockAccountRequest = nameof(UnlockAccountRequest);
        public const string ArchiveAccountRequest = nameof(ArchiveAccountRequest);
        public const string UnarchiveAccountRequest = nameof(UnarchiveAccountRequest);
        public const string ChangeOfContactEmailAddress = nameof(ChangeOfContactEmailAddress);
        public const string ChangeOfOrganisation = nameof(ChangeOfOrganisation);
        public const string ChangeOfRole = nameof(ChangeOfRole);
    }
}