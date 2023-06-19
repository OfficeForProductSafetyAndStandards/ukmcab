using System.Net;
using System.ServiceModel.Syndication;
using UKMCAB.Data.Search.Models;

namespace UKMCAB.Web.UI.Services
{
    public class FeedService : IFeedService
    {
        public SyndicationFeed GetSyndicationFeed(string feedName, HttpRequest request, IEnumerable<CABIndexItem> items, IUrlHelper url)
        {
            var feed = new SyndicationFeed
            {
                Title = new TextSyndicationContent(feedName),
                
            };
                
              //  (feedName, "", new Uri("https://www.gov.uk"));
            feed.Language = "en-US";
            feed.Id = "tag:www.gov.uk,2005:/uk-market-conformity-assessment-bodies";
            feed.Authors.Add(new SyndicationPerson("", "HM Government", ""));
            var feedUrl = request.GetRequestUri();
            var selfLink = new SyndicationLink(feedUrl);
            selfLink.RelationshipType = "self";
            selfLink.MediaType = "application/atom+xml";
            feed.Links.Add(selfLink);

            var feedAbsoluteUri = feedUrl.AbsoluteUri;
            var alternateUrl = new Uri(feedAbsoluteUri.Replace("-feed", string.Empty));
            var alternateLink = new SyndicationLink(alternateUrl);
            alternateLink.RelationshipType = "alternate";
            alternateLink.MediaType = "text/html";
            feed.Links.Add(alternateLink);
            var syndicationItems = new List<SyndicationItem>();
            foreach (var cabIndexItem in items)
            {
                var summaryText = $"<div>{GetAddress(cabIndexItem)}{GetLegislativeAreas(cabIndexItem.LegislativeAreas)}</div>";
                var item = new SyndicationItem
                {
                    Id = $"tag:www.gov.uk,2005:/search/cab-profile/{cabIndexItem.URLSlug}",
                    LastUpdatedTime = cabIndexItem.LastUpdatedDate.GetValueOrDefault(),
                    Links = { GetProfileSyndicationLink(cabIndexItem.URLSlug, request, url) },
                    Title = new TextSyndicationContent(cabIndexItem.Name),
                    Summary = new TextSyndicationContent(summaryText, TextSyndicationContentKind.Html),
                    Content = new TextSyndicationContent(summaryText, TextSyndicationContentKind.Html),
                };
                syndicationItems.Add(item);
            }
            feed.Items = syndicationItems;
            feed.LastUpdatedTime = feed.Items.Any() ? feed.Items.Max(f => f.LastUpdatedTime).DateTime : DateTime.UtcNow;
            return feed;
        }

        private string GetAddress(CABIndexItem item)
        {
            var sb = new StringBuilder("<h2>Address:</h2>");
            sb.AppendFormat("<div>{0}</div>",
                StringExt.Join("<br />", item.AddressLine1, item.AddressLine2, item.TownCity, item.County,
                    item.Postcode, item.Country));
            return sb.ToString();
        }

        private string GetLegislativeAreas(string[] legislativeAreas)
        {
            var sb = new StringBuilder("<h2>Legislative areas:</h2>");
            sb.Append("<div><ul>");
            if (!legislativeAreas.Any())
            {
                sb.Append("<li>Not provided</li>");
            }
            else
            {
                foreach (var legislativeArea in legislativeAreas)
                {
                    sb.AppendFormat("<li>{0}</li>", legislativeArea);
                }
            }
            sb.Append("</ul></div>");
            return sb.ToString();
        }


        private SyndicationLink GetProfileSyndicationLink(string id, HttpRequest request, IUrlHelper url)
        {
            var link = url.Action("Index", "CABProfile", new { Area = "search", id }, request.Scheme, request.GetOriginalHostFromHeaders());
            var returnUrl = WebUtility.UrlEncode(request.GetRequestUri().PathAndQuery.Replace("-feed", string.Empty));
            var profileLink = new SyndicationLink(new Uri($"{link}?returnUrl={returnUrl}"));
            profileLink.RelationshipType = "alternate";
            profileLink.MediaType = "text/html";
            return profileLink;
        }
    }
}
