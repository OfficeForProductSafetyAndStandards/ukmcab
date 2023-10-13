using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using UKMCAB.Common;

namespace UKMCAB.Web;

public static class Ext
{
    public static bool IsInternalRewrite(this HttpContext httpContext) => ((bool?)httpContext.Items["internal"]) == true;
    public static void SetInternalRewrite(this HttpContext httpContext, bool value = true) => httpContext.Items["internal"] = value;

    public static string GetOriginalHostFromHeaders(this HttpRequest request)
    {
        var xOriginalHostHeaderKey = "X-ORIGINAL-HOST";
        if (request.Headers.Any(h => h.Key.Equals(xOriginalHostHeaderKey, StringComparison.InvariantCultureIgnoreCase)))
        {
            return request.Headers.First(h => h.Key.Equals(xOriginalHostHeaderKey, StringComparison.InvariantCultureIgnoreCase)).Value;
        }

        return request.Host.Value;
    }

    public static Uri GetRequestUri(this HttpRequest request)
    {
        var builder = new UriBuilder();
        var hostComponents = request.GetOriginalHostFromHeaders().Split(':');
        builder.Scheme = request.Scheme;
        builder.Host = hostComponents[0];
        builder.Path = request.Path;
        builder.Query = request.QueryString.ToUriComponent();
        if (hostComponents.Length == 2)
        {
            builder.Port = Convert.ToInt32(hostComponents[1]);
        }
        return builder.Uri;
    }

    public static IDictionary<string, string> ToTrackingMetadata(this HttpContext ctx, Dictionary<string,string>? extra = null)
    {
        var dictionary = new Dictionary<string, string>
        {
            [AiTracking.Metadata.UserAgent] = ctx.Request.Headers.UserAgent.ToString(),
            [AiTracking.Metadata.User] = ctx.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "",
        };

        if (extra != null)
        {
            foreach (var kvp in extra)
            {
                dictionary.Add(kvp.Key, kvp.Value);
            }
        }

        return dictionary;
    }
}
