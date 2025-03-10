﻿using System.Net;
using System.ServiceModel.Syndication;
using UKMCAB.Data.Models;
using UKMCAB.Data.Search.Models;

namespace UKMCAB.Web.UI.Services
{
    public class FeedService : IFeedService
    {
        public SyndicationFeed GetSyndicationFeed(string? feedName, HttpRequest request, Document document, IUrlHelper url)
        {
            var feed = GetFeedBody(feedName, request);
            var syndicationItems = new List<SyndicationItem>();
            var summaryText = string.Format("<div>{0}{1}{2}{3}{4}{5}{6}</div>",
                GetFeedElement("Address", document.AddressLine1, document.AddressLine2, document.TownCity, document.County, document.Postcode, document.Country),
                GetFeedElement("Registered office location", document.RegisteredOfficeLocation),
                GetFeedElement("Telephone", document.Phone),
                GetFeedElement("Email address", document.Email),
                GetFeedElement("Website", document.Website),
                GetList("Body types", document.BodyTypes),
                GetLaList("Legislative areas", document.DocumentLegislativeAreas)
                );

            var item = new SyndicationItem
            {
                Id = $"tag:www.gov.uk,2005:/search/cab-profile/{document.URLSlug}",
                LastUpdatedTime = document.LastUpdatedDate,
                Links = { GetProfileSyndicationLink(document.URLSlug, request, url) },
                Title = new TextSyndicationContent(document.Name),
                Summary = new TextSyndicationContent(summaryText, TextSyndicationContentKind.Html),
                Content = new TextSyndicationContent(summaryText, TextSyndicationContentKind.Html),
            };
            syndicationItems.Add(item);
            feed.Items = syndicationItems;
            feed.LastUpdatedTime = feed.Items.Any() ? feed.Items.Max(f => f.LastUpdatedTime).DateTime : DateTime.UtcNow;
            return feed;
        }

        public SyndicationFeed GetSyndicationFeed(string? feedName, HttpRequest request, IEnumerable<CABIndexItem> items, IUrlHelper url)
        {
            var feed = GetFeedBody(feedName, request);
            var syndicationItems = new List<SyndicationItem>();
            foreach (var cabIndexItem in items)
            {
                var summaryText = string.Format("<div>{0}{1}</div>",
                    GetFeedElement("Address", cabIndexItem.AddressLine1, cabIndexItem.AddressLine2, cabIndexItem.TownCity, cabIndexItem.County, cabIndexItem.Postcode, cabIndexItem.Country),
                    GetList("Legislative areas", cabIndexItem.DocumentLegislativeAreas.Select(l => l.LegislativeAreaName)));
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

        private SyndicationFeed GetFeedBody(string? feedName, HttpRequest request)
        {
            var feed = new SyndicationFeed
            {
                Title = new TextSyndicationContent(feedName),
            };

            feed.Language = "en-US";
            feed.Id = "tag:www.gov.uk,2005:/uk-market-conformity-assessment-bodies";
            feed.Authors.Add(new SyndicationPerson("", "HM Government", ""));
            var feedUrl = request.GetRequestUri();
            var selfLink = new SyndicationLink(feedUrl);
            selfLink.RelationshipType = "self";
            selfLink.MediaType = "application/atom+xml";
            feed.Links.Add(selfLink);

            var feedAbsoluteUri = feedUrl.AbsoluteUri;
            var alternateUrl = new Uri(feedAbsoluteUri.Replace("search-feed", string.Empty).Replace("-feed", string.Empty));
            var alternateLink = new SyndicationLink(alternateUrl);
            alternateLink.RelationshipType = "alternate";
            alternateLink.MediaType = "text/html";
            feed.Links.Add(alternateLink);
            return feed;
        }


        private string GetFeedElement(string title, params string?[] content)
        {
            var sb = new StringBuilder($"<h2>{title}:</h2>");
            sb.AppendFormat("<div>{0}</div>",
                content != null && content.Any() ? string.Join("<br />", content) : "Not provided");
            return sb.ToString();
        }

        private string GetList(string title, IEnumerable<string> list)
        {
            var sb = new StringBuilder($"<h2>{title}:</h2>");
            sb.Append("<div><ul>");

            var listItems = list.ToList();
            if (!listItems.Any())

            {
                sb.Append("<li>Not provided</li>");
            }
            else
            {

                foreach (var listItem in listItems)
                {

                    sb.AppendFormat("<li>{0}</li>", listItem);
                }
            }
            sb.Append("</ul></div>");
            return sb.ToString();
        }
        
        private string GetLaList(string title, IEnumerable<DocumentLegislativeArea> list)
        {
            return GetList(title, list.Select(l => l.LegislativeAreaName));
        }


        private SyndicationLink GetProfileSyndicationLink(string id, HttpRequest request, IUrlHelper url)
        {
            var link = url.Action("Index", "CABProfile", new { Area = "search", id }, request.Scheme, request.GetOriginalHostFromHeaders());
            var requestUri = request.GetRequestUri();
            var returnUrl = requestUri.AbsolutePath.Contains("cab-profile", StringComparison.InvariantCultureIgnoreCase) ?
                requestUri.Query :
                "?returnUrl=" +  WebUtility.UrlEncode(requestUri.PathAndQuery.Replace("search-feed", string.Empty));
            var profileLink = new SyndicationLink(new Uri($"{link}{returnUrl}"));
            profileLink.RelationshipType = "alternate";
            profileLink.MediaType = "text/html";
            return profileLink;
        }
    }
}
