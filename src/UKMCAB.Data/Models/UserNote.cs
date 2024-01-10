namespace UKMCAB.Data.Models
{
    public class UserNote
    {
        public UserNote(string userId, string username, string userrole, DateTime date, string note)
        {
            UserId = userId;
            UserName = username;
            UserRole = userrole;
            DateTime = date;
            Note = note;
        }

        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserRole { get; set; }
        public DateTime DateTime { get; set; }
        public string Note { get; set; }
    }
}