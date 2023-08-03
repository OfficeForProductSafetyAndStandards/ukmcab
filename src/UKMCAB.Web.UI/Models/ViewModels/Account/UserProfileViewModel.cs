namespace UKMCAB.Web.UI.Models.ViewModels.Account
{
    public class UserProfileViewModel : ILayoutModel
    {
        public string? Title => "User profile";
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string ContactEmailAddress { get; set; }

        public bool IsEdited { get; set; }
    }
}
