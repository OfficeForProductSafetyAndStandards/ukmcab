using UKMCAB.Data.Models.Users;

namespace UKMCAB.Data.Models
{
    public class Audit
    {
        public Audit() { }


        public Audit(string userId, string username, DateTime date, string status, string comment = null)
        {
            UserId = userId;
            UserName = username;
            DateTime = date;
            Status = status;
            Comment = comment;
        }

        public Audit(UserAccount userAccount, string status, string comment = null) : this(userAccount.Id, $"{userAccount.FirstName} {userAccount.Surname}", DateTime.UtcNow, status, comment) { }

        public string UserId { get; set; }
        public string UserName { get; set; }
        public DateTime DateTime { get; set; }
        public string Status { get; set; }
        public string? Comment { get; set; }
    }

    public class AuditStatus
    {
        public const string Created = nameof(Created);
        public const string Saved = nameof(Saved);
        public const string Published = nameof(Published);
        public const string RePublished = nameof(RePublished);
        public const string Archived = nameof(Archived);
        public const string Unarchived = nameof(Unarchived);
    }

}
