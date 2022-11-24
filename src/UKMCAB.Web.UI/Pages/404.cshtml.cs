using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UKMCAB.Web.UI.Pages
{
    public class _404Model : PageModel, ILayoutModel
    {
        public string? Title => "We can't find that page";

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
