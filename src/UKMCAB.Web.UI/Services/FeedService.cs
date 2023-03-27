using System.ServiceModel.Syndication;
using UKMCAB.Data.Search.Models;

namespace UKMCAB.Web.UI.Services
{
    public class FeedService : IFeedService
    {
        public SyndicationFeed GetSyndicationFeed(HttpRequest request, IEnumerable<CABIndexItem> items, IUrlHelper url)
        {
            var feed = new SyndicationFeed("UK Market Conformity Assessment Bodies", "", new Uri("https://www.gov.uk"));
            feed.Id = "tag:www.gov.uk,2005:/uk-market-conformity-assessment-bodies";
            feed.Authors.Add(new SyndicationPerson("", "HM Government", ""));
            var selfLink = new SyndicationLink(request.GetRequestUri());
            selfLink.RelationshipType = "self";
            selfLink.MediaType = "application/atom+xml";
            feed.Links.Add(selfLink);

            feed.Items = items.Select(c => new SyndicationItem
            {
                Id = $"tag:{request.Host.Value}:/search/cab-profile/{c.CABId}",
                LastUpdatedTime = c.LastUpdatedDate.GetValueOrDefault(),
                Links = { GetProfileSyndicationLink(c.CABId, request, url) },
                Content = SyndicationContent.CreateXmlContent(new AtomFeedContent
                {
                    Id = c.CABId,
                    Name = c.Name,
                    Address = c.Address,
                    LegislativeAreas = c.LegislativeAreas
                }),
                Title = new TextSyndicationContent(c.Name),
                Summary = new TextSyndicationContent(c.Address),
            }).ToList();
            feed.LastUpdatedTime = feed.Items.Max(f => f.LastUpdatedTime).DateTime;
            return feed;
        }


        private SyndicationLink GetProfileSyndicationLink(string id, HttpRequest request, IUrlHelper url)
        {
            var link = url.Action("Index", "CAB", new { Area = "search", id = id }, request.Scheme, request.GetOriginalHostFromHeaders());
            var profileLink = new SyndicationLink(new Uri(link));
            profileLink.RelationshipType = "alternate";
            profileLink.MediaType = "text/html";
            return profileLink;
        }
    }
}
