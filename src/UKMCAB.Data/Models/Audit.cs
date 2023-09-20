using System.Text;
using System.Web;
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Data.Models
{
    public class Audit
    {
        public Audit() { }


        public Audit(string userId, string username, string userrole, DateTime date, string action, string comment = null)
        {
            UserId = userId;
            UserName = username;
            UserRole = userrole;
            DateTime = date;
            Action = action;
            Comment = comment;
        }

        public Audit(UserAccount userAccount, string status, string comment = null) : this(userAccount.Id, $"{userAccount.FirstName} {userAccount.Surname}", userAccount.Role, DateTime.UtcNow, status, comment) { }

        public Audit(UserAccount userAccount, string status, Document publisheDocument, Document previousDocument = null) : this(userAccount.Id, $"{userAccount.FirstName} {userAccount.Surname}", userAccount.Role, DateTime.UtcNow, status)
        {
            var sb = new StringBuilder();
            if (previousDocument == null)
            {
                sb.AppendFormat("<p class=\"govuk-body\">Appointment date: {0}</p>", publisheDocument.AppointmentDate.HasValue ?  publisheDocument.AppointmentDate.Value.ToString("dd/MM/yyyy") + " 12:00" : "Not provided");
                sb.AppendFormat("<p class=\"govuk-body\">Publication date: {0} 12:00</p>", DateTime.UtcNow.ToString("dd/MM/yyyy"));
            }
            else
            {
                var previousSchedules = previousDocument.Schedules ?? new List<FileUpload>();
                var currentSchedules = publisheDocument.Schedules ?? new List<FileUpload>();
                var existingSchedules = currentSchedules.Where(sch => previousSchedules.Any(prev => prev.BlobName.Equals(sch.BlobName)));
                var newSchedules = currentSchedules.Where(sch => previousSchedules.All(prev => !prev.BlobName.Equals(sch.BlobName)));
                var removedSchedules = previousSchedules.Where(sch => currentSchedules.All(pub => !pub.BlobName.Equals(sch.BlobName)));
                foreach (var schedule in existingSchedules)
                {
                    var previousSchedule =
                        previousDocument.Schedules.Single(sch => sch.FileName.Equals(schedule.FileName));
                    if (!previousSchedule.LegislativeArea.Equals(schedule.LegislativeArea))
                    {
                        sb.AppendFormat("<p class=\"govuk-body\">The legislative area {0} has been changed to {1} on this product schedule <a href=\"{2}\" target=\"_blank\" class=\"govuk-link\">{3}</a>.</p>", previousSchedule.LegislativeArea, schedule.LegislativeArea, ScheduleLink(publisheDocument.CABId, schedule.FileName), schedule.Label);
                    }
                    if (!previousSchedule.Label.Equals(schedule.Label))
                    {
                        sb.AppendFormat("<p class=\"govuk-body\">The title of this product schedule <a href=\"{0}\" target=\"_blank\" class=\"govuk-link\">{1}</a> has been changed from {2} to {3}.</p>", ScheduleLink(publisheDocument.CABId, schedule.FileName), schedule.Label, previousSchedule.Label, schedule.Label);
                    }
                }

                foreach (var newSchedule in newSchedules)
                {
                    sb.AppendFormat(
                        "<p class=\"govuk-body\"><a href=\"{0}\" target=\"_blank\" class=\"govuk-link\">{1}</a> was added to the Product schedules page.</p>",
                        ScheduleLink(publisheDocument.CABId, newSchedule.FileName), newSchedule.Label);
                }
                foreach (var removedSchedule in removedSchedules)
                {
                    sb.AppendFormat(
                        "<p class=\"govuk-body\"><a href=\"{0}\" target=\"_blank\" class=\"govuk-link\">{1}</a> was removed from the Product schedules page.</p>",
                        ScheduleLink(publisheDocument.CABId, removedSchedule.FileName), removedSchedule.Label);
                }
            }

            Comment = HttpUtility.HtmlEncode(sb.ToString());
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
    }

    public class AuditActions
    {
        public const string Created = nameof(Created);
        public const string Saved = nameof(Saved);
        public const string Published = nameof(Published);
        public const string RePublished = nameof(RePublished);
        public const string Archived = nameof(Archived);
        public const string Unarchived = nameof(Unarchived);
        public const string UnarchiveRequest = nameof(UnarchiveRequest);
    }

}