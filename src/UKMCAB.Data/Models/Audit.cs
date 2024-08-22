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
            string? comment = null, string? publicComment = null)
        {
            UserId = userId;
            UserName = username;
            UserRole = userrole;
            DateTime = date;
            Action = action;
            Comment = comment;
            PublicComment = publicComment;
        }

        public Audit(UserAccount? userAccount, string action, string? comment = null, string? publicComment = null) : this(userAccount?.Id,
            $"{userAccount?.FirstName} {userAccount?.Surname}", userAccount?.Role, DateTime.UtcNow, action, comment,
            publicComment)
        {
        }

        public Audit(UserAccount? userAccount, string action, Document publishedDocument,
            Document? previousDocument = null, string? comment = null, string? publicComment = null, string? publishType = null) : this(
            userAccount?.Id, $"{userAccount?.FirstName} {userAccount?.Surname}", userAccount?.Role, DateTime.UtcNow,
            action, comment, publicComment)
        {
            var sbInternalComment = new StringBuilder();
            
            HtmlSanitizer htmlSanitizer = new HtmlSanitizer();
            htmlSanitizer.AllowedTags.Clear();
            htmlSanitizer.AllowedTags.Add("br");

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
                sbInternalComment.AppendFormat("<p class=\"govuk-body\">Appointment date: {0}</p>",
                    publishedDocument.AppointmentDate.HasValue
                        ? publishedDocument.AppointmentDate.Value.ToString("dd/MM/yyyy") + " 12:00"
                        : "Not provided");
                sbInternalComment.AppendFormat("<p class=\"govuk-body\">Publication date: {0} 12:00</p>",
                    DateTime.UtcNow.ToString("dd/MM/yyyy"));
                sbInternalComment.Append($"<p class=\"govuk-body\">Added CAB review date {publishedDocument.RenewalDate.ToStringBeisDateFormat()}</p>");
            }
            else
            {
                string[] schedulesAndDocsType = { "Documents", "Schedules" };

                foreach (var docType in schedulesAndDocsType)
                {
                    if (docType.Equals("Documents"))
                    {  
                        CalculateChangesToDocuments(publishedDocument, previousDocument, sbInternalComment);
                    }
                    else
                    { 
                        CalculateChangesToSchedules(publishedDocument, previousDocument, sbInternalComment);
                    }
                }

                CalculateChangesToLegislativeAreas(previousDocument.DocumentLegislativeAreas,
                    publishedDocument.DocumentLegislativeAreas, sbInternalComment);

                CalculateChangesToScopeOfAppointments(previousDocument, publishedDocument, sbInternalComment);

                if (publishedDocument.RenewalDate != null)
                {
                    CalculateChangesToReviewDate(publishedDocument, previousDocument, sbInternalComment);
                }
            }

            if (sbInternalComment.Length > 0)
            {
                sbInternalComment.Insert(0, "<p class=\"govuk-body\">Changes:</p>");                
            }

            publishType = !string.IsNullOrEmpty(publishType) ? $"<p class=\"govuk-body\">{publishType}</p>" : string.Empty;
            comment = !string.IsNullOrEmpty(comment) ? $"<p class=\"govuk-body\">{comment}</p>" : string.Empty;
            publicComment = !string.IsNullOrEmpty(publicComment)
                ? $"<p class=\"govuk-body\">{publicComment}</p>"
                : string.Empty;

            Comment = string.Join("", HttpUtility.HtmlEncode(comment), HttpUtility.HtmlEncode(sbInternalComment.ToString()), HttpUtility.HtmlEncode(publishType));
            PublicComment = HttpUtility.HtmlEncode(publicComment);
        }

        private static void CalculateChangesToReviewDate(Document previousDocument, Document publishedDocument,
            StringBuilder sb)
        {
            if (previousDocument.RenewalDate == null)
            {
                sb.Append($"<p class=\"govuk-body\">Added CAB review date {publishedDocument.RenewalDate.ToStringBeisDateFormat()}</p>");
            }
            else if (previousDocument.RenewalDate != publishedDocument.RenewalDate)
            {
                sb.Append($"<p class=\"govuk-body\">Changed CAB review date {previousDocument.RenewalDate.ToStringBeisDateFormat()} to {publishedDocument.RenewalDate.ToStringBeisDateFormat()}</p>");
            }
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
                var standardName = la.Value.ToString() == DataConstants.LegislativeAreasWithDifferentDataModel.Construction ? 
                    "designated standard" : "scope of appointment";
                if (countScopesAdded > 0)
                {
                    sb.Append(
                        $"<p class=\"govuk-body\">{countScopesAdded} {standardName}{(countScopesAdded > 1 ? "s" : null)} {(countScopesAdded > 1 ? "have" : "has")} been added to {la.Value}.</p>");
                }

                if (countScopesRemoved > 0)
                {
                    sb.Append(
                        $"<p class=\"govuk-body\">{countScopesRemoved} {standardName}{(countScopesRemoved > 1 ? "s" : null)} {(countScopesRemoved > 1 ? "have" : "has")} been removed from {la.Value}.</p>");
                }
            }
        }

        private static void CalculateChangesToLegislativeAreas(List<DocumentLegislativeArea> previousLAs,
            List<DocumentLegislativeArea> currentLAs, StringBuilder sb)
        {
            var currentActiveLAs = currentLAs.Where(n => n.Archived is null or false);
            var currentArchivedLAs = currentLAs.Where(n => n.Archived == true);

            var previousActiveLAs = previousLAs.Where(n => n.Archived is null or false);
            var previousArchivedLAs = previousLAs.Where(n => n.Archived == true);

            var newlyAddedLAs = currentActiveLAs.Where(la => previousLAs.All(pla => pla.LegislativeAreaId != la.LegislativeAreaId));
            var removedLAs =  previousLAs.Where(la => currentLAs.All(cla => cla.LegislativeAreaId != la.LegislativeAreaId));
            var newlyAddedAndArchivedLAs = currentArchivedLAs.Where(la => previousLAs.All(cla => cla.LegislativeAreaId != la.LegislativeAreaId));
            var archivedLAs = currentArchivedLAs.Where(la => previousArchivedLAs.All(cla => cla.LegislativeAreaId != la.LegislativeAreaId)).Except(newlyAddedAndArchivedLAs);            

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

            foreach (var la in archivedLAs)
            {
                sb.AppendFormat(
                    "<p class=\"govuk-body\">{0} was archived from legislative area.</p>",
                    la.LegislativeAreaName);
            }

            foreach (var la in newlyAddedAndArchivedLAs)
            {
                sb.AppendFormat(
                    "<p class=\"govuk-body\">{0} was added and archived from legislative area.</p>",
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

                    if (previousLa.Archived == true && currentLa.Archived == false)
                    {
                        sb.AppendFormat(
                            "<p class=\"govuk-body\">{0} was unarchived from legislative area.</p>",
                            currentLa.LegislativeAreaName);
                    }
                }
            }
        }

        private static void CalculateChangesToSchedules(Document publishedDocument, Document? previousDocument,  StringBuilder sb)
        {   
            var docTypes = "product schedules";
            var docTypeName = "product schedule";
            var docType = "Schedules";

            var newFileUploads = publishedDocument.ActiveSchedules?.Where(pub => previousDocument.Schedules.All(prev => prev.Id != pub.Id));
            var removedFileUploads = previousDocument?.Schedules?.Where(prev =>  publishedDocument.Schedules.All(pub => pub.Id != prev.Id));

            var newlyAddedFileUploadsAndArchived = publishedDocument.ArchivedSchedules.Where(pub => previousDocument.Schedules.All(prev => prev.Id != pub.Id));
            var archivedFileUploads = publishedDocument.ArchivedSchedules.Where(pub => previousDocument.ArchivedSchedules.All(prev => prev.Id != pub.Id)).Except(newlyAddedFileUploadsAndArchived);

            // existing schedules
            if (publishedDocument.Schedules != null && publishedDocument.Schedules.Any())
            {
                foreach (var fileupload in publishedDocument.Schedules)
                {
                    var previousFileUpload = previousDocument.Schedules.FirstOrDefault(sch => sch.Id.Equals(fileupload.Id));

                    if (previousFileUpload != null)
                    {

                        if (!previousFileUpload.LegislativeArea.Equals(fileupload.LegislativeArea))
                        {
                            sb.AppendFormat(
                                "<p class=\"govuk-body\">The legislative area for the product schedule <a href=\"{2}\" target=\"_blank\" class=\"govuk-link\">{3}</a> has been changed from {0} to {1}.</p>",
                                previousFileUpload.LegislativeArea, fileupload.LegislativeArea,
                                ScheduleOrDocumentLink(publishedDocument.CABId, fileupload.FileName, docType),
                                fileupload.Label);
                        }

                        if (!previousFileUpload.Label.Equals(fileupload.Label))
                        {
                            sb.AppendFormat(
                                "<p class=\"govuk-body\">The title of the {0} <a href=\"{1}\" target=\"_blank\" class=\"govuk-link\">{2}</a> has been changed from {3} to {4}.</p>",
                                docTypeName, ScheduleOrDocumentLink(publishedDocument.CABId, fileupload.FileName, docType),
                                fileupload.Label, previousFileUpload.Label, fileupload.Label);
                        }

                        if (!previousFileUpload.FileName.Equals(fileupload.FileName))
                        {
                            sb.AppendFormat(
                                "<p class=\"govuk-body\">The file for <a href=\"{0}\" target=\"_blank\" class=\"govuk-link\">{1}</a> has been replaced.</p>",
                                ScheduleOrDocumentLink(publishedDocument.CABId, fileupload.FileName, docType),
                                fileupload.Label);
                        }

                        if (previousFileUpload.Archived == true && fileupload.Archived == false)
                        {
                            sb.AppendFormat(
                                "<p class=\"govuk-body\">The file for <a href=\"{0}\" target=\"_blank\" class=\"govuk-link\">{1}</a> has been unarchived.</p>",
                                ScheduleOrDocumentLink(publishedDocument.CABId, fileupload.FileName, docType),
                                fileupload.Label);
                        }
                    }
                }
            }

            // new file uploads
            if (newFileUploads != null && newFileUploads.Any())
            {
                // new file uploads
                foreach (var newFileUpload in newFileUploads)
                {
                    sb.AppendFormat(
                        "<p class=\"govuk-body\"><a href=\"{0}\" target=\"_blank\" class=\"govuk-link\">{1}</a> was added to the {2} page.</p>",
                        ScheduleOrDocumentLink(publishedDocument.CABId, newFileUpload.FileName, docType),
                        newFileUpload.Label, docTypes);
                }
            }

            // newly added and then archived file uploads
            if (newlyAddedFileUploadsAndArchived != null && newlyAddedFileUploadsAndArchived.Any())
            {
                foreach (var newAndArchivedFileUpload in newlyAddedFileUploadsAndArchived)
                {
                    sb.AppendFormat(
                        "<p class=\"govuk-body\"><a href=\"{0}\" target=\"_blank\" class=\"govuk-link\">{1}</a> was added and archived from the {2} page.</p>",
                        ScheduleOrDocumentLink(publishedDocument.CABId, newAndArchivedFileUpload.FileName, docType),
                        newAndArchivedFileUpload.Label, docTypes);
                }
            }

            // archived file uploads
            if (archivedFileUploads != null && archivedFileUploads.Any())
            {
                foreach (var archivedFileUpload in archivedFileUploads)
                {
                    sb.AppendFormat(
                        "<p class=\"govuk-body\"><a href=\"{0}\" target=\"_blank\" class=\"govuk-link\">{1}</a> was archived from the {2} page.</p>",
                        ScheduleOrDocumentLink(publishedDocument.CABId, archivedFileUpload.FileName, docType),
                        archivedFileUpload.Label, docTypes);
                }
            }            

            // removed file uploads
            if (removedFileUploads != null && removedFileUploads.Any())
            {
                foreach (var removedFileUpload in removedFileUploads)
                {
                    sb.AppendFormat(
                        "<p class=\"govuk-body\"><a href=\"{0}\" target=\"_blank\" class=\"govuk-link\">{1}</a> was removed from the {2} page.</p>",
                        ScheduleOrDocumentLink(publishedDocument.CABId, removedFileUpload.FileName, docType),
                        removedFileUpload.Label, docTypes);
                }
            }
        }

        private static void CalculateChangesToDocuments(Document publishedDocument, Document? previousDocument, StringBuilder sb)
        {
            var docTypes = "supporting documents";
            var docTypeName = "supporting document";
            var docType = "Documents";

            var newFileUploads = publishedDocument.Documents?.Where(pub => previousDocument.Documents.All(prev => prev.Id != pub.Id));
            var removedFileUploads = previousDocument?.Documents?.Where(prev => publishedDocument.Documents.All(pub => pub.Id != prev.Id));           

            // existing document
            if (publishedDocument.Documents != null && publishedDocument.Documents.Any())
            {
                foreach (var fileupload in publishedDocument.Documents)
                {
                    var previousFileUpload = previousDocument.Documents.FirstOrDefault(sch => sch.Id.Equals(fileupload.Id));

                    if (previousFileUpload != null) 
                    {

                        if (previousFileUpload.Category != null && !previousFileUpload.Category.Equals(fileupload.Category))
                        {
                            sb.AppendFormat(
                                "<p class=\"govuk-body\">The category for the supporting document <a href=\"{2}\" target=\"_blank\" class=\"govuk-link\">{3}</a> has been changed from {0} to {1}.</p>",
                                previousFileUpload.Category, fileupload.Category,
                                ScheduleOrDocumentLink(publishedDocument.CABId, fileupload.FileName, docType),
                                fileupload.Label);
                        }

                        if (!previousFileUpload.Label.Equals(fileupload.Label))
                        {
                            sb.AppendFormat(
                                "<p class=\"govuk-body\">The title of the {0} <a href=\"{1}\" target=\"_blank\" class=\"govuk-link\">{2}</a> has been changed from {3} to {4}.</p>",
                                docTypeName, ScheduleOrDocumentLink(publishedDocument.CABId, fileupload.FileName, docType),
                                fileupload.Label, previousFileUpload.Label, fileupload.Label);
                        }

                        if (!previousFileUpload.FileName.Equals(fileupload.FileName))
                        {
                            sb.AppendFormat(
                                "<p class=\"govuk-body\">The file for <a href=\"{0}\" target=\"_blank\" class=\"govuk-link\">{1}</a> has been replaced.</p>",
                                ScheduleOrDocumentLink(publishedDocument.CABId, fileupload.FileName, docType),
                                fileupload.Label);
                        }
                    }
                }
            }

            // new file uploads
            if (newFileUploads != null && newFileUploads.Any())
            {
                // new file uploads
                foreach (var newFileUpload in newFileUploads)
                {
                    sb.AppendFormat(
                        "<p class=\"govuk-body\"><a href=\"{0}\" target=\"_blank\" class=\"govuk-link\">{1}</a> was added to the {2} page.</p>",
                        ScheduleOrDocumentLink(publishedDocument.CABId, newFileUpload.FileName, docType),
                        newFileUpload.Label, docTypes);
                }
            }

            // removed file uploads
            if (removedFileUploads != null && removedFileUploads.Any())
            {
                foreach (var removedFileUpload in removedFileUploads)
                {
                    sb.AppendFormat(
                        "<p class=\"govuk-body\"><a href=\"{0}\" target=\"_blank\" class=\"govuk-link\">{1}</a> was removed from the {2} page.</p>",
                        ScheduleOrDocumentLink(publishedDocument.CABId, removedFileUpload.FileName, docType),
                        removedFileUpload.Label, docTypes);
                }
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
    }

    public class AuditCABActions
    {
        public const string Created = "Draft created";
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

        public const string LegislativeAreaAdded = "Legislative area added";
        public const string ApproveLegislativeArea = "Legislative area approved";
        public const string DeclineLegislativeArea = "Legislative area declined";
        public const string LegislativeAreaReviewDateAdded = "Legislative area review date added";
        public const string LegislativeAreaReviewDateUpdated = "Legislative area review date updated";
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