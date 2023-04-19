using System.ServiceModel.Syndication;
using System.Xml;
using UKMCAB.Data;
using UKMCAB.Data.Search.Models;
using UKMCAB.Data.Search.Services;
using UKMCAB.Web.UI.Models.ViewModels.Search;
using UKMCAB.Web.UI.Services;

namespace UKMCAB.Web.UI.Areas.Search.Controllers
{
    [Area("search")]
    public class SearchController : Controller
    {
        private readonly ICachedSearchService _cachedSearchService;
        private readonly IFeedService _feedService;

        private static readonly List<string> _select = new()
        {
            nameof(CABIndexItem.CABId),
            nameof(CABIndexItem.Name),
            nameof(CABIndexItem.Address),
            nameof(CABIndexItem.BodyTypes),
            nameof(CABIndexItem.RegisteredOfficeLocation),
            nameof(CABIndexItem.TestingLocations),
            nameof(CABIndexItem.LegislativeAreas),
            nameof(CABIndexItem.LastUpdatedDate),
        };

        public SearchController(ICachedSearchService cachedSearchService, IFeedService feedService)
        {
            _cachedSearchService = cachedSearchService;
            _feedService = feedService;
        }


        [Route("search")]
        public async Task<IActionResult> Index(SearchViewModel model)
        {
            var searchResults = await SearchInternalAsync(model);

            await SetFacetOptions(model);

            model.SearchResults = searchResults.CABs.Select(c => new ResultViewModel(c)).ToList();
            model.Pagination = new PaginationViewModel
            {
                Total = searchResults.Total,
                PageNumber = model.PageNumber
            };

            return View(model);
        }

        /// <summary>
        /// Search API (used by the email subscriptions functionality)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet("~/__api/search")]
        public async Task<IActionResult> GetSearchResultsAsync(SearchViewModel model)
        {
            var searchResults = await SearchInternalAsync(model, x => x.IgnorePaging = true);
            searchResults.CABs.ForEach(x => x.HiddenText = "[omitted]");
            Response.Headers.Add("X-Count", searchResults.Total.ToString());
            return Json(searchResults.CABs);
        }

        private async Task<CABResults> SearchInternalAsync(SearchViewModel model, Action<CABSearchOptions>? configure = null)
        {
            var opt = new CABSearchOptions
            {
                PageNumber = model.PageNumber,
                Keywords = model.Keywords,
                Sort = model.Sort,
                BodyTypesFilter = model.BodyTypes,
                LegislativeAreasFilter = model.LegislativeAreas,
                RegisteredOfficeLocationsFilter = model.RegisteredOfficeLocations,
                TestingLocationsFilter = model.TestingLocations,
                Select = _select,
            };
            configure?.Invoke(opt);
            return await _cachedSearchService.QueryAsync(opt);
        }

        [Route("search-feed")]
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
                TestingLocationsFilter = model.TestingLocations,
                IgnorePaging = true,
                Select = _select,
            });

            var feed = _feedService.GetSyndicationFeed(Request, searchResult.CABs, Url);

            var settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                NewLineHandling = NewLineHandling.Entitize,
                //NewLineOnAttributes = true,
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
                return File(stream.ToArray(), "application/atom+xml;charset=utf-8");
            }
        }

        private SyndicationLink GetProfileSyndicationLink(string id)
        {
            var link = Url.Action("Index", "CAB", new { Area = "search", id = id }, Request.Scheme, Request.GetOriginalHostFromHeaders());
            var profileLink = new SyndicationLink(new Uri(link));
            profileLink.RelationshipType = "alternate";
            profileLink.MediaType = "text/html";
            return profileLink;
        }

        private async Task SetFacetOptions(SearchViewModel model)
        {
            var facets = await _cachedSearchService.GetFacetsAsync();

            facets.LegislativeAreas = facets.LegislativeAreas.Select(la => la.ToSentenceCase()).ToList()!;

            model.BodyTypeOptions = GetFilterOptions(nameof(model.BodyTypes), "Body type", facets.BodyTypes, model.BodyTypes);
            model.LegislativeAreaOptions = GetFilterOptions(nameof(model.LegislativeAreas), "Legislative area", facets.LegislativeAreas, model.LegislativeAreas);
            model.RegisteredOfficeLocationOptions = GetFilterOptions(nameof(model.RegisteredOfficeLocations), "Registered office location", facets.RegisteredOfficeLocation, model.RegisteredOfficeLocations);
            model.TestingLocationOptions = GetFilterOptions(nameof(model.TestingLocations), "Testing location", facets.TestingLocations, model.TestingLocations);
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
    }
}
