using Microsoft.AspNetCore.Mvc;
using UKMCAB.Data;
using UKMCAB.Web.UI.Models.ViewModels;
using UKMCAB.Web.UI.Services;

namespace UKMCAB.Web.UI.Controllers;

[Route("")]
public class HomeController : Controller
{
    private readonly ISearchFilterService _searchFilterService;

    public HomeController(ISearchFilterService searchFilterService)
    {
        _searchFilterService = searchFilterService;
    }
    
    public class Routes
    {
        public const string Index = "home.index";
        public const string Search = "home.search";
        public const string SearchResults = "home.search-results";
        public const string Profile = "home.profile";
    }

    [HttpGet("", Name = Routes.Index)]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("search", Name = Routes.Search)]
    public IActionResult Search()
    {
        return View();
    }

    [HttpGet("results", Name = Routes.SearchResults)]
    public IActionResult SearchResults(SearchResultsViewModel searchResultsViewModel)
    {
        LoadFilters(searchResultsViewModel);

        searchResultsViewModel.SearchResultViewModels = GetSearchResult(searchResultsViewModel);

        return View(searchResultsViewModel);
    }

    private void LoadFilters(SearchResultsViewModel searchResultsViewModel)
    {
        searchResultsViewModel.BodyTypeOptions = _searchFilterService.BodyTypeFilter;
        searchResultsViewModel.RegisteredOfficeLocationOptions = _searchFilterService.RegisteredOfficeLocationFilter;
        searchResultsViewModel.TestingLocationOptions = _searchFilterService.TestingLocationFilter;
        searchResultsViewModel.LegislativeAreaOptions = _searchFilterService.LegislativeAreaFilter;
        searchResultsViewModel.CheckSelecetedItems();
    }
    
    [HttpGet("profile", Name = Routes.Profile)]
    public IActionResult Profile(string id)
    {
        return View(GetProfile(id));
    }

    private List<SearchResultViewModel> GetSearchResult(SearchResultsViewModel searchResultsViewModel)
    {
        var cabData = CabRepository.Search(searchResultsViewModel.Keywords, searchResultsViewModel.TestingLocations,
            searchResultsViewModel.BodyTypes, searchResultsViewModel.RegisteredOfficeLocations,
            searchResultsViewModel.LegislativeAreas);

        return cabData.Select(c => new SearchResultViewModel
        {
            id = c.ExternalID,
            Name = c.Name,
            Address = c.Address,
            Blurb = "TBD",
            BodyType = string.Join(", ", c.BodyType),
            RegisteredOfficeLocation = string.Join(", ", c.RegisteredOfficeLocation),
            TestingLocations = string.Join(", ", c.TestingLocations),
            LegislativeArea = string.Join(", ", c.LegislativeAreas),
        }).ToList();
    }
    
    private ProfileViewModel GetProfile(string id)
    {
        return new ProfileViewModel
        {
            Name = "Five Star Testing Ltd",
            
            Published = "15 Sept 2021",
            LastUpdated = "11 Dec 2021",
            
            Address = "28 Hampton Estate, Wickham, East Sussex, GU29 1NY, UK",
            Website = "www.fivestartesting.co.uk",
            Email = "info@fivestartesting.co.uk",
            Phone = "+44(0)1923 685310",
            
            DetailsOfAppointments = new List<DetailsOfAppointmentViewModel>
            {
                new()
                {
                    Title = "The Noise Emission in the Environment by Equipment for use Outdoors Regulations 2001",
                    AppointmentDetails = new List<AppointmentDetailViewModel>
                    {
                        new()
                        {
                            Products = new ProductsViewModel
                            {
                                Description = "The following equipment subject to noise limits specified in Schedule 1 and defined in Schedule 3 of the regulations:",
                                Products = new List<string>
                                {
                                    "builders’ hoists for the transport of goods (combustion-engine driven)",
                                    "compaction machines (only vibrating and non-vibrating rollers, vibratory plates and vibratory rammers)",
                                    "compressors (<350 kW)",
                                    "construction winches (combustion-engine driven)"
                                }
                            },
                            ApplicableConformityAssessmentProcedures = new List<ApplicableConformityAssessmentProcedureViewModel>
                            {
                                new()
                                {
                                    Title = "Schedule 9",
                                    Description = "Internal control of production with assessment of technical documentation and periodical checking"
                                },
                                new()
                                {
                                    Title = "Schedule 10",
                                    Description = "Unit verification"
                                }
                            }
                        },
                        new()
                        {
                            Products = new ProductsViewModel
                            {
                                Description = "The following equipment subject to noise limits specified in Schedule 1 and defined in Schedule 3 of the regulations:",
                                Products = new List<string>
                                {
                                    "loaders (<500 kW)",
                                    "mobile cranes",
                                    "motor hoes (<3 kW)",
                                    "paver-finishers (excluding paver-finishers equipped with a high-compaction screed)"
                                }
                            },
                            ApplicableConformityAssessmentProcedures = new List<ApplicableConformityAssessmentProcedureViewModel>
                            {
                                new()
                                {
                                    Title = "Schedule 11",
                                    Description = "Unit verification"
                                }
                            }
                        }
                    }
                },
                new()
                {
                    Title = "The Supply of Machinery (Safety) Regulations 2008",
                    AppointmentDetails = new List<AppointmentDetailViewModel>
                    {
                        new()
                        {
                            Products = new ProductsViewModel
                            {
                                Description = "The following equipment subject to noise limits specified in Schedule 1 and defined in Schedule 3 of the regulations:",
                                Products = new List<string>
                                {
                                    "dozers (<500 kW)",
                                    "dumpers (<500 kW)",
                                    "lawn trimmers/lawn edge trimmers",
                                    "welding generators"
                                }
                            },
                            ApplicableConformityAssessmentProcedures = new List<ApplicableConformityAssessmentProcedureViewModel>
                            {
                                new()
                                {
                                    Title = "Schedule 11",
                                    Description = "Unit verification"
                                },
                                new()
                                {
                                    Title = "Schedule 12",
                                    Description = "Internal control of production with assessment of technical documentation and periodical checking"
                                }
                            }
                        }
                    }
                }
            },
            
            BodyNumber = "2369",
            BodyType = "Approved body, NI Notified body",
            RegisteredOfficeLocation = "United Kingdom",
            TestingLocations = "United Kingdom",
            AccreditaionBody = "United Kingdom Accreditation Service (UKAS)<br />2 Pine Trees, Chertsey Lane, Staines-upon-Thames, TW18 3HR, UK",
            AccreditationStandard = "ISO/IEC 17025:2017",
            AppointmentDetails = @"<p>The scope of the accreditation covers the product categories and conformity assessment procedures concerned in this appointment.</p>" +
                                   "<p>Having considered the recommendation made to the Department for Business, Energy and Industrial Strategy the Secretary of State has appointed the above Conformity Assessment Body to assess the conformity of the product categories and for the modules identified below.</p>" +
                                   "<h3 class=\"govuk-heading-s\">Northern Ireland</h3>" +
                                   "<p>In addition the body is appointed to act for the purposes of conformity assessment of products for Northern Ireland, the scope and extent of appointment mirroring the categories and conformity assessment activities indicates below. This will require the Company to perform conformity assessment in line with EU requirements for goods assessed for the Northern Ireland market.</p>",
            Locations = "Five Star Testing Ltd, 28 Hampton Estate, Wickham, East Sussex, GU29 1NY, UK",
            AppointmentRevisions = new List<AppointmentRevisionViewModel>
            {
                new()
                {
                    Version = "1.0",
                    Date = "15 September 2021",
                    Details = "Issue date"
                },
                new()
                {
                    Version = "2.0",
                    Date = "11 December 2021",
                    Details = "Details fo appointment edited"
                }
            }
        };
    }
}
