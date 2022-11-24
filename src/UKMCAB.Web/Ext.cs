using Microsoft.AspNetCore.Http;

namespace UKMCAB.Web;

public static class Ext
{
    public static bool IsInternalRewrite(this HttpContext httpContext) => ((bool?)httpContext.Items["internal"]) == true;
    public static void SetInternalRewrite(this HttpContext httpContext, bool value = true) => httpContext.Items["internal"] = value;
}
