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

            feed.Items = items.Select(c => new SyndicationItem
            {
                Id = $"tag:www.gov.uk,2005:/search/cab-profile/{c.URLSlug}",
                LastUpdatedTime = c.LastUpdatedDate.GetValueOrDefault(),
                Links = { GetProfileSyndicationLink(c.URLSlug, request, url) },
                Title = new TextSyndicationContent(c.Name),
                Summary = new TextSyndicationContent(StringExt.Join(", ", c.AddressLine1, c.AddressLine2, c.TownCity, c.County, c.Postcode, c.Country), TextSyndicationContentKind.Html),
                Content = new TextSyndicationContent(GetLegislativeAreas(c.LegislativeAreas), TextSyndicationContentKind.Html)
            }).ToList();
            feed.LastUpdatedTime = feed.Items.Any() ? feed.Items.Max(f => f.LastUpdatedTime).DateTime : DateTime.UtcNow;
            return feed;
        }

        private string GetLegislativeAreas(string[] legislativeAreas)
        {
            var sb = new StringBuilder("<h2>Legislative areas</h2>");
            sb.Append("<ul>");
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
            sb.Append("</ul>");
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
