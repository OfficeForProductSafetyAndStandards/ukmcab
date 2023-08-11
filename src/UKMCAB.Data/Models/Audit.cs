using UKMCAB.Data.Models.Users;

namespace UKMCAB.Data.Models
{
    public class Audit
    {
        public Audit() { }


        public Audit(string userId, string username, DateTime date)
        {
            UserId = userId;
            UserName = username;
            DateTime = date;
        }

        public Audit(UserAccount userAccount) : this(userAccount.Id, $"{userAccount.FirstName} {userAccount.Surname}", DateTime.UtcNow) { }

        public string UserId { get; set; }
        public string UserName { get; set; }
        public DateTime DateTime { get; set; }
    }
}
