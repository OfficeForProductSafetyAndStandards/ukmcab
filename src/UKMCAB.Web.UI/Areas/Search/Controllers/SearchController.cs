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
                PageNumber = model.PageNumber
            });


            model.BodyTypeOptions = Constants.Lists.BodyTypes;
            model.RegisteredOfficeLocationOptions = Constants.Lists.Countries;
            model.TestingLocationOptions = Constants.Lists.Countries;
            model.LegislativeAreaOptions = Constants.Lists.LegislativeAreas;
            model.SearchResults = searchResult.CABs.Select(c => new ResultViewModel
            {
                id = c.id,
                Name = c.Name,
                Address = c.Address,
                BodyType = ListToString(c.BodyTypes),
                RegisteredOfficeLocation = c.RegisteredOfficeLocation,
                RegisteredTestLocation = string.Empty,
                LegislativeArea = ListToString(c.LegislativeAreas)
            }).ToList();
            model.Pagination = new PaginationViewModel
            {
                Total = searchResult.Total,
                PageNumber = model.PageNumber
            };

            return View(model);
        }

        private string ListToString(List<string> list)
        {
            if (list == null || list.Count == 0)
            {
                return string.Empty;
            }

            if (list.Count == 1)
            {
                return list.First();
            }

            return $"{list.First()} and {list.Count - 1} other{(list.Count > 2 ? "s" : string.Empty)}";
        }
    }
}
