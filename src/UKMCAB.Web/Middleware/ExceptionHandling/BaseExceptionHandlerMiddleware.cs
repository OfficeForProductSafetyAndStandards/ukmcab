using Microsoft.AspNetCore.Http;
using UKMCAB.Common;

namespace UKMCAB.Web.Middleware.ExceptionHandling;

public abstract class BaseExceptionHandlerMiddleware
{
    public static bool DoesRequireJsonResponse(HttpContext context)
        => context.Request.GetTypedHeaders().Accept.Any(t => t.Suffix.Value.DoesEqual("json")
        || t.SubTypeWithoutSuffix.Value.DoesEqual("json"));

    public static bool DoesRequireTextResponse(HttpContext context)
        => context.Request.GetTypedHeaders().Accept.Any(t => t.MediaType.Value.DoesEqual("text/plain"));
}