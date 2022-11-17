using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UKMCAB.Web.UI.Pages
{
    public class LoggedOffModel : PageModel, ILayoutModel
    {
        public string? Title => "Signed out";

        public void OnGet()
        {
        }
    }
}
