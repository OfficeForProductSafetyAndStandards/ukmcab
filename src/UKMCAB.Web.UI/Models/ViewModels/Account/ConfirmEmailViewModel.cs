namespace UKMCAB.Web.UI.Models.ViewModels.Account
{
    public class ConfirmEmailViewModel : ILayoutModel
    {
        public string Message { get; set; }
        public string? Title => "Confirm email";
    }
}
