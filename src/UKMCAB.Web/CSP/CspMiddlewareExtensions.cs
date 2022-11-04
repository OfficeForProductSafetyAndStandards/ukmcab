using System;
using Microsoft.AspNetCore.Builder;

namespace UKMCAB.Web.CSP;

public static class CspMiddlewareExtensions
{
    public static IApplicationBuilder UseCsp(this IApplicationBuilder applicationBuilder, CspHeader header)
    {
        ArgumentNullException.ThrowIfNull(applicationBuilder);
        ArgumentNullException.ThrowIfNull(header);
        return applicationBuilder.UseMiddleware<CspMiddleware>(header);
    }
}
