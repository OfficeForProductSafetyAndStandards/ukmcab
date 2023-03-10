
namespace UKMCAB.Web.UI.Models.ViewModels.Account
{
    public class LogoutViewModel: ILayoutModel
    {
        public string? Title => "Login";
        public string ReturnURL { get; set;}
    }
}
