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