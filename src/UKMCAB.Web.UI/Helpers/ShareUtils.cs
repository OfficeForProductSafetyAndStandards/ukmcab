﻿using System.Net;
using UKMCAB.Web.UI.Areas.Home.Controllers;
using UKMCAB.Web.UI.Areas.Search.Controllers;
using UKMCAB.Web.UI.Models.ViewModels.Shared;

namespace UKMCAB.Web.UI.Helpers
{
    public class ShareUtils
    {
        public static void AddDetails(HttpContext context, FeedLinksViewModel feedLinksViewModel)
        {
            if (context != null)
            {
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
                    title = $"{feedLinksViewModel.CABName} on the {Constants.SiteName} service.";
                    emailSubject = $"I am sharing {feedLinksViewModel.CABName} profile from the {Constants.SiteName} service";
                    emailBody = $"{title} {encodedUrlNoQueryStrings}";
                    shareUrl = encodedUrlNoQueryStrings;
                }
                else if (endPoint.Equals(SearchController.Routes.Search))
                {
                    title = string.IsNullOrEmpty(feedLinksViewModel.SearchKeyword) ? $"Search results for CABs on the {Constants.SiteName} service." : $"Search results for \"{feedLinksViewModel.SearchKeyword}\" on the {Constants.SiteName} service.";
                    emailSubject = $"I am sharing these search results from the {Constants.SiteName} service";
                    emailBody = $"{title} {encodedUrlWithQueryStrings}";
                    shareUrl = encodedUrlWithQueryStrings;
                }
                else if (endPoint.Equals(HomeController.Routes.Updates))
                {
                    title = $"Service updates and releases page on the {Constants.SiteName} service.";
                    emailSubject = $"I am sharing the service updates and releases page from the {Constants.SiteName} service";
                    emailBody = $"The service updates and releases page on the {Constants.SiteName} service. {encodedUrlNoQueryStrings}";
                    shareUrl = encodedUrlNoQueryStrings;
                }

                feedLinksViewModel.Title = title;
                feedLinksViewModel.EmailSubject = emailSubject;
                feedLinksViewModel.EmailBody = emailBody;
                feedLinksViewModel.ShareUrl = shareUrl;
                feedLinksViewModel.Endpoint = endPoint;
            }


        }
    }
}
