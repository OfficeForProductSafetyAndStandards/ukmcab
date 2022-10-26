using UKMCAB.Data.CosmosDb.Models;
using UKMCAB.Data.CosmosDb.Services;
using UKMCAB.Web.UI.Models;
using UKMCAB.Web.UI.Models.ViewModels;

namespace UKMCAB.Web.UI.Services;

public class CABSearchService : ICABSearchService
{
    private readonly ICosmosDbService _cosmosDbService;

    public CABSearchService(ICosmosDbService cosmosDbService)
    {
        _cosmosDbService = cosmosDbService;
    }

    public async Task<List<CAB>> SearchCABsAsync(string text, FilterSelections filterSelections)
    {
        var cabs = await _cosmosDbService.Query(text);
        cabs = ApplyFilters(cabs, filterSelections);
        return cabs;
    }

    private List<CAB> ApplyFilters(List<CAB> cabs, FilterSelections filterSelections)
    {
        if (filterSelections.BodyTypes != null && filterSelections.BodyTypes.Any())
        {
            cabs = cabs.Where(c => !string.IsNullOrWhiteSpace(c.BodyType) &&
                                   filterSelections.BodyTypes.Any(bt =>
                                       c.BodyType.Contains(bt, StringComparison.InvariantCultureIgnoreCase))).ToList();
        }
        if (filterSelections.RegisteredOfficeLocations != null && filterSelections.RegisteredOfficeLocations.Any())
        {
            cabs = cabs.Where(c => !string.IsNullOrWhiteSpace(c.RegisteredOfficeLocation) &&
                                   filterSelections.RegisteredOfficeLocations.Any(rol =>
                                       c.RegisteredOfficeLocation.Contains(rol, StringComparison.InvariantCultureIgnoreCase))).ToList();
        }
        if (filterSelections.TestingLocations != null && filterSelections.TestingLocations.Any())
        {
            cabs = cabs.Where(c => !string.IsNullOrWhiteSpace(c.TestingLocations) &&
                                   filterSelections.TestingLocations.Any(tl =>
                                       c.TestingLocations.Contains(tl, StringComparison.InvariantCultureIgnoreCase))).ToList();
        }
        if (filterSelections.Regulations != null && filterSelections.Regulations.Any())
        {
            cabs = cabs.Where(c =>
                filterSelections.Regulations.Any(fr =>
                    c.Regulations.Any(r => r.Name.Equals(fr, StringComparison.InvariantCultureIgnoreCase)))).ToList();
        }
        return cabs;
    }

    public async Task<CABProfileViewModel> GetCABAsync(string id)
    {
        var cabData = await _cosmosDbService.GetByIdAsync(id);
        
        var cabProfile = new CABProfileViewModel
        {
            Name = cabData.Name,
            
            Published = "xxx", // todo
            LastUpdated = "xxx", // todo
            
            Address = cabData.Address,
            Website = cabData.Website,
            Email = cabData.Email,
            Phone = cabData.Phone,

            BodyNumber = cabData.BodyNumber,
            BodyType = string.Join(", ", cabData.BodyType),
            RegisteredOfficeLocation = string.Join(", ", cabData.RegisteredOfficeLocation),
            TestingLocations = string.Join(", ", cabData.TestingLocations),
            AppointmentRevisions =  new List<AppointmentRevisionViewModel> // todo
            {
                new()
                {
                    Version = "xxx",
                    Date = "xxx",
                    Details = "xxx"
                }
            }
        };
        cabProfile.Regulations = cabData.Regulations != null
            ? GetRegulations(cabData.Regulations)
            : new List<RegulationViewModel>();
        cabProfile.CertificationActivityLocations = new List<string>(); // TODO: not in new data source


        return cabProfile;
    }

    private List<RegulationViewModel> GetRegulations(List<Regulation> cabDataRegulations)
    {
        var regulations = new List<RegulationViewModel>();
        foreach (var cabDataRegulation in cabDataRegulations)
        {
            regulations.Add(new RegulationViewModel()
            {
                Title = cabDataRegulation.Name,
                Products = cabDataRegulation.Products != null
                                ? GetProducts(cabDataRegulation.Products)
                                : new List<ProductViewModel>()
            });
        }
        return regulations;
    }

    private List<ProductViewModel> GetProducts(List<Product> productGroups)
    {
        var products = new List<ProductViewModel>();
        foreach (var product in productGroups)
        {
            products.Add(new ProductViewModel()
            {
                Name = product.Name,
                Code = product.Code,
                Part = product.PartName,
                Module = product.ModuleName,
                Schedule = product.ScheduleName,
                StandardsSpecification = product.StandardsNumber
            });
        }
        return products;
    }
}