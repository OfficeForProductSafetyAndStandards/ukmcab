using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Org.BouncyCastle.Asn1.Cms;
using System.Security.Claims;
using UKMCAB.Common;

namespace UKMCAB.Web;

public static class Tracking
{
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
