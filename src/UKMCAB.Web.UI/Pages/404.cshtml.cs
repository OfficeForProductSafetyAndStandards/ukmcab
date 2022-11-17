using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UKMCAB.Web.UI.Pages
{
    public class _404Model : PageModel, ILayoutModel
    {
        public string? Title => "We can't find that page";

        public void OnGet()
        {
        }
    }
}
