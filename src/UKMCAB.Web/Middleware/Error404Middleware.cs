using Microsoft.AspNetCore.Http;

namespace UKMCAB.Web.Middleware;

public class PageNotFoundMiddleware
{
    private readonly RequestDelegate _next;
    private readonly HttpErrorOptions _httpErrorOptions;

    public PageNotFoundMiddleware(RequestDelegate next, HttpErrorOptions? httpErrorOptions = null)
    {
        _next = next;
        _httpErrorOptions = httpErrorOptions ?? new();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);
        if (context.Response.StatusCode == StatusCodes.Status404NotFound)
        {
            context.SetInternalRewrite(true);
            context.Items["originalPath"] = context.Request.Path.Value;
            context.Request.Path = _httpErrorOptions.Error404Path;
            await _next(context);
        }
    }
}
