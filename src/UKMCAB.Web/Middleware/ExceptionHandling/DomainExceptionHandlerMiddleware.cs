using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using UKMCAB.Common;
using UKMCAB.Common.Exceptions;

namespace UKMCAB.Web.Middleware.ExceptionHandling;

public class DomainExceptionHandlerMiddleware : BaseExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly HttpErrorOptions _httpErrorOptions;

    public DomainExceptionHandlerMiddleware(RequestDelegate next, HttpErrorOptions? httpErrorOptions = null)
    {
        _next = next;
        _httpErrorOptions = httpErrorOptions ?? new();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (PermissionDeniedException pex)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            if (DoesRequireJsonResponse(context))
            {
                context.Response.ContentType = "application/json; charset=utf-8";
                var pd = new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Error",
                    Detail = pex.Message
                };
                await context.Response.WriteAsync(pd.Serialize(), Encoding.UTF8);
            }
            else
            {
                context.Response.Clear();
                context.SetEndpoint(endpoint: null);
                var routeValuesFeature = context.Features.Get<IRouteValuesFeature>();
                routeValuesFeature?.RouteValues?.Clear();

                context.Features.Set<IExceptionHandlerPathFeature>(new ExceptionHandlerFeature
                {
                    Error = pex,
                    Path = context.Request.Path,
                });
                context.Request.Method = "GET";
                context.Request.Path = _httpErrorOptions.Error403Path;

                await _next(context);
            }
        }
        catch (DomainException dex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            if (DoesRequireJsonResponse(context))
            {
                context.Response.ContentType = "application/json; charset=utf-8";
                var pd = new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Error",
                    Detail = dex.Message
                };
                await context.Response.WriteAsync(pd.Serialize(), Encoding.UTF8);
            }
            else
            {
                context.Response.Clear();
                context.SetEndpoint(endpoint: null);
                var routeValuesFeature = context.Features.Get<IRouteValuesFeature>();
                routeValuesFeature?.RouteValues?.Clear();

                context.Features.Set<IExceptionHandlerPathFeature>(new ExceptionHandlerFeature
                {
                    Error = dex,
                    Path = context.Request.Path,
                });
                context.Request.Method = "GET";
                context.Request.Path = _httpErrorOptions.Error400Path; //Constants.PathError400DomainException;
                context.Response.StatusCode = StatusCodes.Status400BadRequest;

                await _next(context);
            }
        }
    }
}
