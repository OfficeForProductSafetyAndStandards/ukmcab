using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UKMCAB.Web.UI.Pages
{
    public class _403Model : PageModel, ILayoutModel
    {
        public string? Title => "Permission denied"; // authenticated, but not permitted.

        public IActionResult OnGet()
        {
            if (HttpContext.IsInternalRewrite())
            {
                return Page();
            }
            else
            {
                HttpContext.SetEndpoint(endpoint: null);
                return NotFound();
            }
        }
    }
}
