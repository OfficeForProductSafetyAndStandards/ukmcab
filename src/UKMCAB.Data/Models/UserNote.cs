namespace UKMCAB.Data.Models
{
    public class UserNote
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserRole { get; set; }
        public DateTime DateTime { get; set; }
        public string Note { get; set; }
    }
}