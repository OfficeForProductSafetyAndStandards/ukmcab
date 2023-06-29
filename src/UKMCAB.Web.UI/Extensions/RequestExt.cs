using System.Web;

namespace UKMCAB.Web.UI.Extensions
{
    public static class RequestExt
    {
        public static string RemoveQueryParameters(this HttpRequest request, params string[] queryKeys)
        {
            var queryItems = request.QueryString;
            var collection = HttpUtility.ParseQueryString(queryItems.Value);
            foreach (var queryKey in queryKeys)
            {
                collection.Remove(queryKey);
            }
            return collection.HasKeys() ? request.Path + "?" + collection : request.Path;
        }
        public static string RemoveQueryParameters(this string queryString, params string[] queryKeys)
        {
            var collection = HttpUtility.ParseQueryString(queryString);
            foreach (var queryKey in queryKeys)
            {
                collection.Remove(queryKey);
            }
            return collection.HasKeys() ? "?" + collection.ToString() : string.Empty;
        }
    }
}
