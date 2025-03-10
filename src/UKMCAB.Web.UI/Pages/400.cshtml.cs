using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UKMCAB.Web.UI.Pages
{
    /// <summary>
    /// Domain/business exception
    /// </summary>
    public class _400Model : PageModel, ILayoutModel
    {
        public string? Title => "There was a problem"; 

        public Exception? Exception { get; set; }

        public IActionResult OnGet()
        {
            if (HttpContext.IsInternalRewrite())
            {
                var exceptionHandler = HttpContext?.Features?.Get<IExceptionHandlerPathFeature>();
                Exception = exceptionHandler?.Error;
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
