using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UKMCAB.Web.UI.Pages
{
    public class _401Model : PageModel, ILayoutModel
    {
        public string? Title => "Access denied"; // no authentication.

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
