using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text;
using UKMCAB.Common;
using UKMCAB.Infrastructure.Logging;
using UKMCAB.Infrastructure.Logging.Models;

namespace UKMCAB.Web.Middleware.ExceptionHandling;

/// <summary>
/// Responsible for logging and handling unexpected exceptions.
/// </summary>
public class UnexpectedExceptionHandlerMiddleware : BaseExceptionHandlerMiddleware
{
    public record UnhandledExceptionData(string ReferenceId);

    private readonly RequestDelegate _next;
    private readonly ILogger<UnexpectedExceptionHandlerMiddleware> _logger;
    private readonly ILoggingService _loggingService;
    private readonly HttpErrorOptions _httpErrorOptions;

    public UnexpectedExceptionHandlerMiddleware(RequestDelegate next, ILogger<UnexpectedExceptionHandlerMiddleware> logger, ILoggingService loggingService, HttpErrorOptions? httpErrorOptions = null)
    {
        _next = next;
        _logger = logger;
        _loggingService = loggingService;
        _httpErrorOptions = httpErrorOptions ?? new();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            var logEntry = GetLogEntry(context, ex);
            var referenceId = _loggingService.Log(logEntry);
            _logger.LogError(ex, ex.Message);

            if (DoesRequireJsonResponse(context))
            {
                await HandleJsonResponseAsync(context, referenceId, ex);
            }
            else if (DoesRequireTextResponse(context))
            {
                await HandleTextResponseAsync(context, referenceId, ex);
            }
            else
            {

                await HandleExceptionAsync(context, ex, referenceId);
            }

        }
    }

    private static async Task HandleTextResponseAsync(HttpContext context, string referenceId, Exception ex)
    {
        context.Response.ContentType = "text/plain; charset=utf-8";
        var pd = CreateProblemDetails(referenceId, ex);

        var serializer = new YamlDotNet.Serialization.SerializerBuilder()
            .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance).Build();
        var yaml = serializer.Serialize(pd);

        await context.Response.WriteAsync(yaml, Encoding.UTF8);
    }


    private static async Task HandleJsonResponseAsync(HttpContext context, string referenceId, Exception ex)
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        var pd = CreateProblemDetails(referenceId, ex);
        await context.Response.WriteAsync(pd.Serialize(), Encoding.UTF8);
    }

    private static ProblemDetails CreateProblemDetails(string referenceId, Exception ex)
    {
        var pd = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An error has occurred.  Please try again later.",
            Detail = "Feel free to contact technical support and provide the reference code: " + referenceId
        }.Chain(pd => pd.Extensions.Add("reference", referenceId));

        if (ThisBuild.IsDebug())
        {
            pd.Extensions.Add("Exception", ex.ToString());
        }

        return pd;
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex, string referenceId)
    {
        var originalPath = context.Request.Path;
        context.Request.Path = _httpErrorOptions.Error500Path;
        context.SetInternalRewrite(true);
        context.Request.Method = "GET";

        try
        {
            // Critical
            context.Response.Clear();
            context.SetEndpoint(endpoint: null);
            var routeValuesFeature = context.Features.Get<IRouteValuesFeature>();
            routeValuesFeature?.RouteValues?.Clear();
            // [END] critical

            context.Features.Set(new UnhandledExceptionData(referenceId));

            var exceptionHandlerFeature = new ExceptionHandlerFeature()
            {
                Error = ex,
                Path = originalPath.Value ?? string.Empty,
            };
            context.Features.Set<IExceptionHandlerFeature>(exceptionHandlerFeature);
            context.Features.Set<IExceptionHandlerPathFeature>(exceptionHandlerFeature);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await _next(context);

            return;
        }
        catch (Exception ex2)
        {
            var referenceId2 = _loggingService.Log(new LogEntry(ex2));
            _logger.LogError(ex2, ex.Message);
            context.Response.ContentType = "text/plain; charset=utf-8";
            await context.Response.WriteAsync($"A catastrophic failure occurred.  Please provide reference code {referenceId2} to the support team");
        }
        finally
        {
            context.Request.Path = originalPath;
        }

    }

    private static LogEntry GetLogEntry(HttpContext httpContext, Exception exception)
    {
        var req = httpContext?.Request;

        return new LogEntry
        {
            IPAddress = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? string.Empty,
            HttpMethod = req?.Method ?? string.Empty,
            Message = exception?.Message ?? string.Empty,
            Url = req?.Path + req?.QueryString ?? string.Empty,
            UrlReferrer = req?.Headers["Referer"].ToString() ?? string.Empty,
            UserAgent = req?.Headers["User-Agent"].ToString() ?? string.Empty,
            //UserData = principal?.GetRawData() ?? string.Empty,
            ExceptionData = exception?.ToString() ?? string.Empty
        };
    }
}
