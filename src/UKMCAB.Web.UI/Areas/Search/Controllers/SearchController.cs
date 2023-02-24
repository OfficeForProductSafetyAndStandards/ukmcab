using UKMCAB.Data.Search.Models;
using UKMCAB.Data.Search.Services;
using UKMCAB.Web.UI.Models.ViewModels.Search;

namespace UKMCAB.Web.UI.Areas.Search.Controllers
{
    [Area("search")]
    public class SearchController : Controller
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }


        [Route("search")]
        public async Task<IActionResult> Index(SearchViewModel model)
        {
            if (model.PageNumber == 0)
            {
                model.PageNumber = 1;
            }
            var searchResult = await _searchService.QueryAsync(new CABSearchOptions
            {
                PageNumber = model.PageNumber,
                Keywords = model.Keywords,
                BodyTypesFilter = model.BodyTypes,
                LegislativeAreasFilter = model.LegislativeAreas,
                RegisteredOfficeLocationsFilter = model.RegisteredOfficeLocations,
                TestingLocationsFilter = model.TestingLocations
            });

            var facets = await _searchService.GetFacetsAsync();

            model.BodyTypeOptions = GetFilterOptions(nameof(model.BodyTypes), facets.BodyTypes, model.BodyTypes);
            model.LegislativeAreaOptions = GetFilterOptions(nameof(model.LegislativeAreas), facets.LegislativeAreas, model.LegislativeAreas);
            model.RegisteredOfficeLocationOptions = GetFilterOptions(nameof(model.RegisteredOfficeLocations), facets.RegisteredOfficeLocation, model.RegisteredOfficeLocations);
            model.TestingLocationOptions = GetFilterOptions(nameof(model.TestingLocations), facets.TestingLocations, model.TestingLocations);

            model.SearchResults = searchResult.CABs.Select(c => new ResultViewModel
            {
                id = c.id,
                Name = c.Name,
                Address = c.Address,
                BodyType = ListToString(c.BodyTypes),
                RegisteredOfficeLocation = c.RegisteredOfficeLocation,
                RegisteredTestLocation = ListToString(c.TestingLocations),
                LegislativeArea = ListToString(c.LegislativeAreas)
            }).ToList();
            model.Pagination = new PaginationViewModel
            {
                Total = searchResult.Total,
                PageNumber = model.PageNumber
            };

            return View(model);
        }

        private List<FilterOption> GetFilterOptions(string facetName, IEnumerable<string> facets, IEnumerable<string> selectedFacets)
        {
            if (selectedFacets == null)
            {
                selectedFacets = Array.Empty<string>();
            }
            return facets.Select(f => new FilterOption(facetName, f,
                selectedFacets.Any(sf => sf.Equals(f, StringComparison.InvariantCultureIgnoreCase)))).ToList();
        }

        private string ListToString(string[] list)
        {
            if (list == null || list.Length == 0)
            {
                return string.Empty;
            }

            if (list.Length == 1)
            {
                return list.First();
            }

            return $"{list.First()} and {list.Length - 1} other{(list.Length > 2 ? "s" : string.Empty)}";
        }
    }
}
