namespace UKMCAB.Core.Models.Account
{
    public class RegistrationDTO
    {
        public string UserRole { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Reason { get; set; }
        public List<string> LegislativeAreas { get; set; }
    }
}
