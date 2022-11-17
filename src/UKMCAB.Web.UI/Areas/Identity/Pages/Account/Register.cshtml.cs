using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UKMCAB.Web.UI.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel, ILayoutModel
    {
        public string? Title => "Register";
        public void OnGet()
        {
        }
    }
}
