using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Polly;
using static UKMCAB.Web.Middleware.ExceptionHandling.UnexpectedExceptionHandlerMiddleware;

namespace UKMCAB.Web.UI.Pages
{
    public class _500Model : PageModel, ILayoutModel
    {
        public string? Title => "System error";

        public Exception? Exception { get; set; }

        public string? ErrorCode { get; set; }
        public string? ErrorContext { get; set; }


        public IActionResult OnGet()
        {
            if (HttpContext.IsInternalRewrite())
            {
                var exceptionHandler = HttpContext?.Features?.Get<IExceptionHandlerFeature>();
                var data = HttpContext?.Features?.Get<UnhandledExceptionData>();

                Exception = exceptionHandler?.Error;
                ErrorCode = data?.ReferenceId;
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
