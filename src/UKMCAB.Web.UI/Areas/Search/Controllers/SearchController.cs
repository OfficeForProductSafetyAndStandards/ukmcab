using System.Xml;
using UKMCAB.Data;
using UKMCAB.Data.Search.Models;
using UKMCAB.Data.Search.Services;
using UKMCAB.Web.Middleware.BasicAuthentication;
using UKMCAB.Subscriptions.Core.Integration.CabService;
using UKMCAB.Web.UI.Models.ViewModels.Search;
using UKMCAB.Web.UI.Models.ViewModels.Shared;
using UKMCAB.Web.UI.Services;

namespace UKMCAB.Web.UI.Areas.Search.Controllers
{
    [Area("search")]
    public class SearchController : Controller
    {
        private readonly ICachedSearchService _cachedSearchService;
        private readonly IFeedService _feedService;
        private readonly BasicAuthenticationOptions _basicAuthOptions;

        private static readonly List<string> _select = new()
        {
            nameof(CABIndexItem.CABId),
            nameof(CABIndexItem.Name),
            nameof(CABIndexItem.AddressLine1),
            nameof(CABIndexItem.AddressLine2),
            nameof(CABIndexItem.TownCity),
            nameof(CABIndexItem.Postcode),
            nameof(CABIndexItem.Country),
            nameof(CABIndexItem.BodyTypes),
            nameof(CABIndexItem.RegisteredOfficeLocation),
            nameof(CABIndexItem.TestingLocations),
            nameof(CABIndexItem.LegislativeAreas),
            nameof(CABIndexItem.LastUpdatedDate),
        };

        public SearchController(ICachedSearchService cachedSearchService, IFeedService feedService, BasicAuthenticationOptions basicAuthOptions)
        {
            _cachedSearchService = cachedSearchService;
            _feedService = feedService;
            _basicAuthOptions = basicAuthOptions;
        }


        [Route("search")]
        public async Task<IActionResult> Index(SearchViewModel model)
        {
            var searchResults = await SearchInternalAsync(_cachedSearchService, model);

            await SetFacetOptions(model);

            model.SearchResults = searchResults.CABs.Select(c => new ResultViewModel(c)).ToList();
            model.Pagination = new PaginationViewModel
            {
                Total = searchResults.Total,
                PageNumber = model.PageNumber,
                ResultsPerPage = DataConstants.Search.SearchResultsPerPage,
                ResultType = "bodies"
            };

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
            var searchResults = await SearchInternalAsync(_cachedSearchService, model, x => x.IgnorePaging = true);
            searchResults.CABs.ForEach(x => x.HiddenText = "[omitted]");
            Response.Headers.Add("X-Count", searchResults.Total.ToString());
            return Json(searchResults.CABs.Select(x => new SubscriptionsCoreCabSearchResultModel { CabId = Guid.Parse(x.CABId), Name = x.Name }));
        }

        internal static async Task<CABResults> SearchInternalAsync(ICachedSearchService cachedSearchService, SearchViewModel model, Action<CABSearchOptions>? configure = null)
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
            return await cachedSearchService.QueryAsync(opt);
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

        [Route("cache-clear")]
        public async Task<IActionResult> ClearCache(string password)
        {
            if (password == _basicAuthOptions.Password)
            {
                await _cachedSearchService.ClearAsync();
                
                await Task.Delay(1000);
                return RedirectToAction("Index");
            }

            return BadRequest();
        }
    }
}
