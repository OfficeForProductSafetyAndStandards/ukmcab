namespace UKMCAB.Web.UI.Models.ViewModels.Account
{
    public class LogoutViewModel: ILayoutModel
    {
        public string? Title => "Logout";
        public string ReturnURL { get; set;}
    }
}
