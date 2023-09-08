using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UKMCAB.Common;
using UKMCAB.Common.Exceptions;
using UKMCAB.Infrastructure.Logging;
using UKMCAB.Web.Middleware.ExceptionHandling;

namespace UKMCAB.Web.Middleware;
public static class HttpErrorHandlingMiddlewareExtensions
{
    /// <summary>
    /// Configures custom HTTP error/exception handling
    /// </summary>
    /// <param name="app"></param>
    /// <param name="hostingEnvironment"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    /// <exception cref="DomainException"></exception>
    /// <exception cref="PermissionDeniedException"></exception>
    /// <remarks>Call BEFORE UseRouting</remarks>
    public static IApplicationBuilder UseCustomHttpErrorHandling(this IApplicationBuilder app, IConfiguration configuration)
    {
        app.UseMiddleware<PageNotFoundMiddleware>();
        app.UseMiddleware<UnexpectedExceptionHandlerMiddleware>();
        app.UseMiddleware<DomainExceptionHandlerMiddleware>();

        if (configuration.GetValue<bool>("EnableExceptionHandlingDiagnostics", true))
        {
            const string p = "/__diag/cmd/http-error-handling/";
            app.Map($"{p}unhandledex", x => x.Run(ctx => throw new Exception("Test unhandled exception"))); // /__diag/cmd/http-error-handling/unhandledex
            app.Map($"{p}domainex", x => x.Run(ctx => throw new DomainException("Test domain exception"))); // /__diag/cmd/http-error-handling/domainex
            app.Map($"{p}permissiondenied", x => x.Run(ctx => throw new PermissionDeniedException("Test domain exception"))); // /__diag/cmd/http-error-handling/permissiondenied

            app.Map($"{p}flush-errors", x => x.Run(async context =>
            {
                await context.Response.WriteAsJsonAsync(x.ApplicationServices.GetRequiredService<ILoggingService>().FlushErrors.Serialize());
            }));

            app.Map($"{p}current-snapshot", x => x.Run(async context =>
            {
                await context.Response.WriteAsJsonAsync(x.ApplicationServices.GetRequiredService<ILoggingService>().Snapshot.Serialize());
            }));

            /*
                URLS:
                /__diag/cmd/http-error-handling/unhandledex
                /__diag/cmd/http-error-handling/domainex
                /__diag/cmd/http-error-handling/permissiondenied
                /__diag/cmd/http-error-handling/flush-errors
                /__diag/cmd/http-error-handling/current-snapshot
            */
        }

        return app;
    }

    public static IServiceCollection AddCustomHttpErrorHandling(this IServiceCollection services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddHostedService<ExceptionLogFlusherHostedService>();


        return services;
    }
}
