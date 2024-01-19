using System.Text;
using System.Web;
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Data.Models
{
    public class Audit
    {
        public Audit() { }

        public Audit(string userId, string username, string userrole, DateTime date, string action, string? comment = null, string? publicComment = null, bool isUserInputComment = true)
        {
            UserId = userId;
            UserName = username;
            UserRole = userrole;
            DateTime = date;
            Action = action;
            Comment = comment;
            PublicComment = publicComment;
            IsUserInputComment = isUserInputComment;
        }

        public Audit(UserAccount? userAccount, string action, string? comment = null, string? publicComment = null) : this(userAccount?.Id, $"{userAccount?.FirstName} {userAccount?.Surname}", userAccount?.Role, DateTime.UtcNow, action, comment, publicComment) { }

        public Audit(UserAccount? userAccount, string action, Document publishedDocument, Document? previousDocument = null, string? comment = null, string? publicComment = null) : this(userAccount?.Id, $"{userAccount?.FirstName} {userAccount?.Surname}", userAccount?.Role, DateTime.UtcNow, action, comment, publicComment)
        {
            var sb = new StringBuilder();
            if (previousDocument == null)
            {
                sb.AppendFormat("<p class=\"govuk-body\">Appointment date: {0}</p>", publishedDocument.AppointmentDate.HasValue ? publishedDocument.AppointmentDate.Value.ToString("dd/MM/yyyy") + " 12:00" : "Not provided");
                sb.AppendFormat("<p class=\"govuk-body\">Publication date: {0} 12:00</p>", DateTime.UtcNow.ToString("dd/MM/yyyy"));
            }
            else
            {
                var previousSchedules = previousDocument.Schedules ?? new List<FileUpload>();
                var currentSchedules = publishedDocument.Schedules ?? new List<FileUpload>();
                var existingSchedules = currentSchedules.Where(sch => previousSchedules.Any(prev => prev.UploadDateTime.Equals(sch.UploadDateTime)));
                var newSchedules = currentSchedules.Where(sch => previousSchedules.All(prev => !prev.UploadDateTime.Equals(sch.UploadDateTime)));
                var removedSchedules = previousSchedules.Where(sch => currentSchedules.All(pub => !pub.UploadDateTime.Equals(sch.UploadDateTime)));
                foreach (var schedule in existingSchedules)
                {
                    var previousSchedule =
                        previousDocument.Schedules.Single(sch => sch.UploadDateTime.Equals(schedule.UploadDateTime));
                    if (!previousSchedule.LegislativeArea.Equals(schedule.LegislativeArea))
                    {
                        sb.AppendFormat("<p class=\"govuk-body\">The legislative area {0} has been changed to {1} on this product schedule <a href=\"{2}\" target=\"_blank\" class=\"govuk-link\">{3}</a>.</p>", previousSchedule.LegislativeArea, schedule.LegislativeArea, ScheduleLink(publishedDocument.CABId, schedule.FileName), schedule.Label);
                    }
                    if (!previousSchedule.Label.Equals(schedule.Label))
                    {
                        sb.AppendFormat("<p class=\"govuk-body\">The title of this product schedule <a href=\"{0}\" target=\"_blank\" class=\"govuk-link\">{1}</a> has been changed from {2} to {3}.</p>", ScheduleLink(publishedDocument.CABId, schedule.FileName), schedule.Label, previousSchedule.Label, schedule.Label);
                    }
                }

                foreach (var newSchedule in newSchedules)
                {
                    sb.AppendFormat(
                        "<p class=\"govuk-body\"><a href=\"{0}\" target=\"_blank\" class=\"govuk-link\">{1}</a> was added to the Product schedules page.</p>",
                        ScheduleLink(publishedDocument.CABId, newSchedule.FileName), newSchedule.Label);
                }
                foreach (var removedSchedule in removedSchedules)
                {
                    sb.AppendFormat(
                        "<p class=\"govuk-body\"><a href=\"{0}\" target=\"_blank\" class=\"govuk-link\">{1}</a> was removed from the Product schedules page.</p>",
                        ScheduleLink(publishedDocument.CABId, removedSchedule.FileName), removedSchedule.Label);
                }
            }

            if (sb.Length > 0)
            {
                sb.Insert(0, "<p class=\"govuk-body\">Changes:</p>");
            }

            comment = !string.IsNullOrEmpty(comment) ? $"<p class=\"govuk-body\">{comment}</p>" : string.Empty;

            Comment = string.Join("", comment, HttpUtility.HtmlEncode(sb.ToString()));
            IsUserInputComment = false;
        }

        private static string ScheduleLink(string cabId, string fileName)
        {
            return $"/search/cab-schedule-view/{cabId}?file={fileName}&filetype=schedules";
        }

        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserRole { get; set; }
        public DateTime DateTime { get; set; }
        public string Action { get; set; }
        public string? Comment { get; set; }
        public string? PublicComment { get; set; }
        public bool? IsUserInputComment { get; set; }
    }

    public class AuditCABActions
    {
        public const string Created = nameof(Created);
        public const string Saved = nameof(Saved);
        public const string Published = nameof(Published);
        public const string RePublished = nameof(RePublished);
        public const string Archived = nameof(Archived); 
        public const string Unarchived = nameof(Unarchived); // Used on a draft that has been unarchived
        public const string UnarchiveRequest = nameof(UnarchiveRequest); //Used for CAB's that have been unarchived (previous and new draft version)
        public const string UnarchiveApprovalRequest = "Request to unarchive"; // Ukas request for unarchive
        public const string SubmittedForApproval = "Submitted for OPSS approval"; // UKAS request to publish
        public const string CABApproved = "CAB Approved";
        public const string CABDeclined = "CAB Declined";
        public const string DraftDeleted = "Draft deleted";
        public const string UnpublishApprovalRequest = "Request to unpublish"; // UKAS request to un publish and create draft
        public const string ArchiveApprovalRequest = "Request to archive"; // UKAS request to archive
        public const string UnArchiveApprovalRequestDeclined = "CAB unarchive declined";  // Ukas request for unarchive declined
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