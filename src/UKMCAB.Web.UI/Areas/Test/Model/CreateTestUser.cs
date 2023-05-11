namespace UKMCAB.Web.UI.Areas.Test.Model
{
    public class CreateTestUser : ApiRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
