using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static UKMCAB.Web.Middleware.ExceptionHandling.UnexpectedExceptionHandlerMiddleware;

namespace UKMCAB.Web.UI.Pages.bdc39f6c9d90
{
    public class _500Model : PageModel, ILayoutModel
    {
        public string? Title => "System error";

        public Exception? Exception { get; set; }

        public string? ErrorCode { get; set; }
        public string? ErrorContext { get; set; }


        public void OnGet()
        {
            var exceptionHandler = HttpContext?.Features?.Get<IExceptionHandlerFeature>();
            var data = HttpContext?.Features?.Get<UnhandledExceptionData>();
            
            Exception = exceptionHandler?.Error;
            ErrorCode = data?.ReferenceId;
        }
    }
}
