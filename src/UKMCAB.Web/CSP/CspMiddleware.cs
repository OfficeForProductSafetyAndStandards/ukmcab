using Microsoft.AspNetCore.Http;

namespace UKMCAB.Web.CSP;

public class CspMiddleware
{
    private readonly CspHeader _header;
    private readonly RequestDelegate _next;
    public CspMiddleware(RequestDelegate next, CspHeader header)
    {
        if (next == null)
        {
            throw new ArgumentNullException(nameof(next));
        }

        if (header == null)
        {
            throw new ArgumentNullException(nameof(header));
        }

        _next = next;
        _header = header;
    }

    public async Task Invoke(HttpContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var response = context.Response;

        if (response == null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        _header.AddHeader(response.Headers);

        await _next(context);
    }
}