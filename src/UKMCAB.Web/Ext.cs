using Microsoft.AspNetCore.Http;

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
}
