namespace UKMCAB.Web.UI.Models.ViewModels.Cookies
{
    public class CookiesViewModel : ILayoutModel
    {
        public string Title => Constants.PageTitle.CookiesPolicy;
        public string? Cookies { get; set; }
        public string? ReturnURL { get; set; }
    }
}
