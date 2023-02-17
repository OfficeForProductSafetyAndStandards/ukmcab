using UKMCAB.Web.UI.Models.ViewModels.Search;

namespace UKMCAB.Web.UI.Areas.Search.Controllers
{
    [Area("search")]
    public class SearchController : Controller
    {
        [Route("search")]
        public IActionResult Index(SearchViewModel model)
        {
            model = new SearchViewModel
            {
                BodyTypeOptions = Constants.Lists.BodyTypes,
                RegisteredOfficeLocationOptions = Constants.Lists.Countries,
                TestingLocationOptions = Constants.Lists.Countries,
                LegislativeAreaOptions = Constants.Lists.LegislativeAreas,
                SearchResults = FakeResults()
            };
            return View(model);
        }
        private List<ResultViewModel> FakeResults()
        {
            var results = new List<ResultViewModel>();
            for (int i = 0; i < 20; i++)
            {
                results.Add(new ResultViewModel
                {
                    Name = "Gateshead MBC (Tyne and Wear Trading Standards)" + i.ToString(),
                    Address = "Trading Standards, Civic Centre, Regent Street, Gateshead, Tyne & Wear, NE8 1HH",
                    BodyType = "Approved body and 1 others",
                    RegisteredOfficeLocation = "United Kingdom",
                    RegisteredTestLocation = "United Kingdom",
                    LegislativeArea = "Measuring instructions and 1 others"
                });
            }

            return results;
        }
    }
}
