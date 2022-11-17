using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UKMCAB.Web.UI.Pages.bdc39f6c9d90
{
    public class _403Model : PageModel, ILayoutModel
    {
        public string? Title => "Permission denied"; // authenticated, but not permitted.

        public void OnGet()
        {
        }
    }
}
