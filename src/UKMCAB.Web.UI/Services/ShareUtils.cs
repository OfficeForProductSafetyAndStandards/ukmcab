using System.Net;
using UKMCAB.Web.UI.Areas.Home.Controllers;
using UKMCAB.Web.UI.Areas.Search.Controllers;
using UKMCAB.Web.UI.Models.ViewModels.Shared;

namespace UKMCAB.Web.UI.Services
{
    public class ShareUtils
    {
        public static void AddDetails(HttpContext context, FeedLinksViewModel feedLinksViewModel)
        {
            if (context != null) {
                var requestPath = context.Request.Path.ToString();
                var requestSchemeHost = $"{context.Request.Scheme}://{context.Request.Host}";
                var encodedUrlWithQueryStrings = WebUtility.UrlEncode(requestSchemeHost + context.Request.GetRequestUri().PathAndQuery);
                var encodedUrlNoQueryStrings = WebUtility.UrlEncode(requestSchemeHost + requestPath);

                
                var title = Constants.SiteName;
                var emailSubject = $"I'm sharing this from UKMCAB.";
                var emailBody = $"{title}. {encodedUrlWithQueryStrings}";
                var shareUrl = string.Empty;
                var endPoint = context?.GetEndpoint()?.Metadata?.GetMetadata<IRouteNameMetadata>()?.RouteName ?? string.Empty;

                if (endPoint.Equals(CABProfileController.Routes.CabDetails))
                {
                    title = $"{feedLinksViewModel.CABName} on {Constants.SiteName}.";
                    emailSubject = $"I am sharing {feedLinksViewModel.CABName} profile from {Constants.SiteName}.";
                    emailBody = $"{title} {encodedUrlNoQueryStrings}";
                    shareUrl = encodedUrlNoQueryStrings;
                }
                else if (endPoint.Equals(SearchController.Routes.Search))
                {
                    title = string.IsNullOrEmpty(feedLinksViewModel.SearchKeyword) ? $"Search results for CABs on {Constants.SiteName}." : $"Search results for {feedLinksViewModel.SearchKeyword} on {Constants.SiteName}.";
                    emailSubject = $"I am sharing these search results from {Constants.SiteName}.";
                    emailBody = $"{title} {encodedUrlWithQueryStrings}";
                    shareUrl = encodedUrlWithQueryStrings;
                }
                else if (endPoint.Equals(HomeController.Routes.Updates))
                {
                    title = $"Service updates and releases page on {Constants.SiteName}.";
                    emailSubject = $"I am sharing the service updates and releases page from {Constants.SiteName}.";
                    emailBody = $"The service updates and releases page on {Constants.SiteName}. {encodedUrlNoQueryStrings}";
                    shareUrl = encodedUrlNoQueryStrings;
                }

                feedLinksViewModel.Title = title ;
                feedLinksViewModel.EmailSubject = emailSubject ;
                feedLinksViewModel.EmailBody = emailBody ;
                feedLinksViewModel.ShareUrl = shareUrl ;
                feedLinksViewModel.Endpoint = endPoint;
            }

            
        }
    }
}
