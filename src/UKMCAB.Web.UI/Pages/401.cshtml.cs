using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UKMCAB.Web.UI.Pages.bdc39f6c9d90
{
    public class _401Model : PageModel, ILayoutModel
    {
        public string? Title => "Access denied"; // no authentication.

        public void OnGet()
        {
        }
    }
}
