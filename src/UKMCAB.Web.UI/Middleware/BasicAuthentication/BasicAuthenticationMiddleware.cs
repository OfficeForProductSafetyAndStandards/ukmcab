using Microsoft.Net.Http.Headers;
using System.Net.Http.Headers;
using System.Text;

namespace UKMCAB.Web.UI.Middleware.BasicAuthentication;

public class BasicAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly BasicAuthenticationOptions _basicAuthOptions;

    public BasicAuthenticationMiddleware(RequestDelegate next, BasicAuthenticationOptions basicAuthOptions)
    {
        _next = next;
        _basicAuthOptions = basicAuthOptions;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (_basicAuthOptions.IsEnabled)
        {
            if (AuthenticationHeaderValue.TryParse(context.Request.Headers.Authorization, out var authHeader))
            {
                if (authHeader.Scheme == "Basic")
                {
                    var (userName, password) = GetCredentials(authHeader);
                    if (userName == _basicAuthOptions.UserName && password == _basicAuthOptions.Password)
                    {
                        await _next(context);
                    }
                    else
                    {
                        Challenge(context);
                    }
                }
                else if (authHeader.Scheme.StartsWith("apikey"))
                {
                    if (!context.Request.Path.StartsWithSegments("/api"))
                    {
                        Challenge(context);
                    }
                    else
                    {
                        await _next(context);
                    }
                }
            }
            else
            {
                Challenge(context);
            }
        }
        else
        {
            await _next(context);
        }
    }

    private static (string? userName, string? password) GetCredentials(AuthenticationHeaderValue? authHeader)
    {
        try
        {
            var credentialBytes = Convert.FromBase64String(authHeader.Parameter!);
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
            return (credentials[0], credentials[1]);
        }
        catch
        {
            return (null, null);
        }
    }

    private void Challenge(HttpContext context)
    {
        context.Response.StatusCode = 401;
        context.Response.Headers.Append(HeaderNames.WWWAuthenticate, "Basic realm=\"" + context.Request.Host + "\"");
    }
}
