namespace UKMCAB.Web.UI.Models.ViewModels.Account
{
    public class UserProfileViewModel : ILayoutModel
    {
        public string? Title => "My details";
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ContactEmailAddress { get; set; }
        public string Role { get; set; }

        public bool IsEdited { get; set; }
        public string? OrganisationName { get; set; }
        public DateTime? LastLogonUtc { get; set; }
    }
}
