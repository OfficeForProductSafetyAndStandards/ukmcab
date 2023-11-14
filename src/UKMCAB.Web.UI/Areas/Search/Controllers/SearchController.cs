using System.Net;
using System.Xml;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using UKMCAB.Core.Security;
using UKMCAB.Data;
using UKMCAB.Data.Search.Models;
using UKMCAB.Data.Search.Services;
using UKMCAB.Subscriptions.Core.Integration.CabService;
using UKMCAB.Web.Middleware.BasicAuthentication;
using UKMCAB.Web.Security;
using UKMCAB.Web.UI.Areas.Subscriptions.Controllers;
using UKMCAB.Web.UI.Extensions;
using UKMCAB.Web.UI.Helpers;
using UKMCAB.Web.UI.Models.ViewModels.Search;
using UKMCAB.Web.UI.Models.ViewModels.Shared;
using UKMCAB.Web.UI.Services;

namespace UKMCAB.Web.UI.Areas.Search.Controllers
{
    [Area("search"), Route("search")]
    public class SearchController : Controller
    {
        private readonly ICachedSearchService _cachedSearchService;
        private readonly IFeedService _feedService;
        private readonly BasicAuthenticationOptions _basicAuthOptions;
        private readonly TelemetryClient _telemetry;
        private readonly IOptionsMonitor<OpenIdConnectOptions> _options;
        private static readonly List<string> _select = new()
        {
            nameof(CABIndexItem.CABId),
            nameof(CABIndexItem.Status),
            nameof(CABIndexItem.Name),
            nameof(CABIndexItem.URLSlug),
            nameof(CABIndexItem.AddressLine1),
            nameof(CABIndexItem.AddressLine2),
            nameof(CABIndexItem.TownCity),
            nameof(CABIndexItem.County),
            nameof(CABIndexItem.Postcode),
            nameof(CABIndexItem.Country),
            nameof(CABIndexItem.BodyTypes),
            nameof(CABIndexItem.RegisteredOfficeLocation),
            nameof(CABIndexItem.TestingLocations),
            nameof(CABIndexItem.LegislativeAreas),
            nameof(CABIndexItem.LastUpdatedDate),
            nameof(CABIndexItem.LastUserGroup)
        };

        public static class Routes
        {
            public const string Search = "search.index";
            public const string SearchFeed = "search.feed";
        }
        public SearchController(ICachedSearchService cachedSearchService, IFeedService feedService, BasicAuthenticationOptions basicAuthOptions, TelemetryClient telemetry, IOptionsMonitor<OpenIdConnectOptions> options)
        {
            _options = options;
            _cachedSearchService = cachedSearchService;
            _feedService = feedService;
            _basicAuthOptions = basicAuthOptions;
            _telemetry = telemetry;
        }


        [Route("/", Name = Routes.Search)]
        public async Task<IActionResult> Index(SearchViewModel model, string? state = null)
        {
            if (state != null)
            {
                return Redirect(OneLoginHelper.LogoutCallbackPath + "?state=" + state);
            }

            var internalSearch = User != null && User.Identity.IsAuthenticated;
            model.Sort ??= internalSearch && string.IsNullOrWhiteSpace(model.Keywords) ? DataConstants.SortOptions.A2ZSort : DataConstants.SortOptions.Default;
            var searchResults = await SearchInternalAsync(_cachedSearchService, model, internalSearch: internalSearch);
            model.InternalSearch = internalSearch;
            model.IsOPSSUser = User != null && User.IsInRole(Roles.OPSS.Id);
            await SetFacetOptions(model);

            model.ReturnUrl = WebUtility.UrlEncode(HttpContext.Request.GetRequestUri().PathAndQuery);

            model.SearchResults = searchResults.CABs.Select(c => new ResultViewModel(c)).ToList();
            model.Pagination = new PaginationViewModel
            {
                Total = searchResults.Total,
                PageNumber = model.PageNumber,
                ResultsPerPage = DataConstants.Search.SearchResultsPerPage,
                ResultType = "bodies"
            };
            model.FeedLinksViewModel = new FeedLinksViewModel
            {
                FeedUrl = Url.RouteUrl(Routes.SearchFeed) ?? throw new InvalidOperationException(),
                EmailUrl = Url.RouteUrl(SubscriptionsController.Routes
                    .Step0RequestSearchSubscription) ?? throw new InvalidOperationException(),
                SearchKeyword = model.Keywords ?? string.Empty
            };

            ShareUtils.AddDetails(HttpContext, model.FeedLinksViewModel);

            if (Request.QueryString.HasValue)
            {
                model.FeedLinksViewModel.FeedUrl += Request.QueryString.Value.EnsureStartsWith("?").RemoveQueryParameters("pagenumber", "sort");
                model.FeedLinksViewModel.EmailUrl += Request.QueryString.Value.EnsureStartsWith("?");
            }


            _telemetry.TrackEvent(AiTracking.Events.CabsSearched, HttpContext.ToTrackingMetadata(new()
            {
                [AiTracking.Metadata.ResultsCount] = searchResults.Total.ToString(),
                [AiTracking.Metadata.WithCriteria] = (
                        model.Keywords.Clean() != null
                    || (model.LegislativeAreas ?? Array.Empty<string>()).Length > 0
                    || (model.RegisteredOfficeLocations ?? Array.Empty<string>()).Length > 0
                    || (model.BodyTypes ?? Array.Empty<string>()).Length > 0
                    ).ToString().ToLower(),
            }));

            return View(model);
        }

        /// <summary>
        /// CAB search API used by the Email Subscriptions Core
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet("~/__api/subscriptions/core/cab-search")]
        public async Task<IActionResult> GetSearchResultsAsync(SearchViewModel model)
        {
            var searchResults = await SearchInternalAsync(_cachedSearchService, model, configure: x => x.IgnorePaging = true);
            searchResults.CABs.OrderBy(x => x.Name).ForEach(x => x.HiddenText = "[omitted]");
            Response.Headers.Add("X-Count", searchResults.Total.ToString());
            return Json(searchResults.CABs.Select(x => new SubscriptionsCoreCabSearchResultModel { CabId = x.CABId.ToGuid()??throw new Exception($"Cannot convert to guid '{x.CABId}'"), Name = x.Name }));
        }

        internal static async Task<CABResults> SearchInternalAsync(ICachedSearchService cachedSearchService, SearchViewModel model, bool internalSearch = false, Action<CABSearchOptions>? configure = null)
        {
            var opt = new CABSearchOptions
            {
                PageNumber = model.PageNumber,
                Keywords = model.Keywords,
                Sort = model.Sort,
                BodyTypesFilter = model.BodyTypes,
                LegislativeAreasFilter = model.LegislativeAreas,
                RegisteredOfficeLocationsFilter = model.RegisteredOfficeLocations,
                StatusesFilter = model.Statuses,
                UserGroupsFilter = model.UserGroups,
                Select = _select,
                InternalSearch = internalSearch
            };
            configure?.Invoke(opt);
            return await cachedSearchService.QueryAsync(opt);
        }

        [Route("search-feed", Name = Routes.SearchFeed)]
        public async Task<IActionResult> AtomFeed(SearchViewModel model)
        {
            model.Sort = DataConstants.SortOptions.LastUpdated;
            var searchResult = await _cachedSearchService.QueryAsync(new CABSearchOptions
            {
                PageNumber = model.PageNumber,
                Keywords = model.Keywords,
                Sort = model.Sort,
                BodyTypesFilter = model.BodyTypes,
                LegislativeAreasFilter = model.LegislativeAreas,
                RegisteredOfficeLocationsFilter = model.RegisteredOfficeLocations,
                IgnorePaging = true,
                Select = _select,
            });

            var feed = _feedService.GetSyndicationFeed(GetFeedName(model), Request, searchResult.CABs, Url);

            var settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                NewLineHandling = NewLineHandling.Entitize,
                Indent = true,
                ConformanceLevel = ConformanceLevel.Document
            };

            using (var stream = new MemoryStream())
            {
                using (var xmlWriter = XmlWriter.Create(stream, settings))
                {
                    feed.GetAtom10Formatter().WriteTo(xmlWriter);
                    xmlWriter.Flush();
                }
                stream.Position = 0;
                var content = new StreamReader(stream).ReadToEnd();
                return Content(content, "application/atom+xml;charset=utf-8");
            }
        }

        private string? GetFeedName(SearchViewModel model)
        {
            return string.IsNullOrEmpty(model.Keywords) ? "UKMCAB search results" : $"UKMCAB search results for \"{model.Keywords.Trim()}\"";
        }

        private async Task SetFacetOptions(SearchViewModel model)
        {
            var facets = await _cachedSearchService.GetFacetsAsync(model.InternalSearch);

            facets.LegislativeAreas = facets.LegislativeAreas.Select(la => la.ToSentenceCase()).ToList()!;

            model.BodyTypeOptions = GetFilterOptions(nameof(model.BodyTypes), "Body type", facets.BodyTypes, model.BodyTypes);
            model.LegislativeAreaOptions = GetFilterOptions(nameof(model.LegislativeAreas), "Legislative area", facets.LegislativeAreas, model.LegislativeAreas);
            model.RegisteredOfficeLocationOptions = GetFilterOptions(nameof(model.RegisteredOfficeLocations), "Registered office location", facets.RegisteredOfficeLocation, model.RegisteredOfficeLocations);
            if (model.InternalSearch)
            {
                model.StatusOptions = GetFilterOptions(nameof(model.Statuses), "Status", facets.StatusValue, model.Statuses);
                model.LastUserGroupOptions = GetFilterOptions(nameof(model.UserGroups), "User groups", facets.LastUserGroup, model.UserGroups);
            }
        }

        private FilterViewModel GetFilterOptions(string facetName, string facetLabel, IEnumerable<string> facets, IEnumerable<string> selectedFacets)
        {
            var filter = new FilterViewModel
            {
                Id = facetName,
                Label = facetLabel
            };
            if (selectedFacets == null)
            {
                selectedFacets = Array.Empty<string>();
            }
            filter.FilterOptions = facets.Select(f => new FilterOption(facetName, f,
                selectedFacets.Any(sf => sf.Equals(f, StringComparison.InvariantCultureIgnoreCase)))).ToList();
            return filter;
        }

        [Route("cache-clear")]
        public async Task<IActionResult> ClearCache(string password)
        {
            if (password == _basicAuthOptions.Password)
            {
                await _cachedSearchService.ClearAsync();
                return RedirectToAction("Index");
            }

            return BadRequest();
        }
    }
}
