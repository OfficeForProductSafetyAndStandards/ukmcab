using UKMCAB.Identity.Stores.CosmosDB;

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

        public Audit(UKMCABUser user, DateTime date) : this(user.Id, $"{user.FirstName} {user.LastName}", date)
        {
            
        }


        public string UserId { get; set; }
        public string UserName { get; set; }
        public DateTime DateTime { get; set; }
    }
}
