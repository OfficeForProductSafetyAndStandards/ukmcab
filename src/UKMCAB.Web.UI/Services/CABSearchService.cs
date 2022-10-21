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
                    Version = "TBD",
                    Date = "TBD",
                    Details = "TBD"
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
            var regulation = new RegulationViewModel()
            {
                Title = cabDataRegulation.Name,
                Description = string.Empty, // TODO: not in new data source
            };
            regulation.ProductGroups = cabDataRegulation.Products != null
                ? GetProductGroups(cabDataRegulation.Products)
                : new List<ProductsGroupViewModel>();
            
            regulations.Add(regulation);
        }

        return regulations;
    }

    private List<ProductsGroupViewModel> GetProductGroups(List<Product> productGroups)
    {
        var products = new List<ProductsGroupViewModel>();
        foreach (var product in productGroups)
        {
            var productGroup = new ProductsGroupViewModel()
            {
                Title = product.Name,
                Description = string.Empty, // TODO: not in new data source
                Products = new List<string>() {product.Name},// TODO: not in new data source
                Schedules = new List<ScheduleViewModel>(),
                StandardsSpecifications = new List<StandardSpecificationViewModel>()
            };
            if (product.ScheduleName != null)
            {
                productGroup.Schedules = new List<ScheduleViewModel>
                {
                    new ScheduleViewModel
                    {
                        Title = product.ScheduleName,
                        Description = string.Empty, // TODO: not in new data source
                        Label = string.Empty, // TODO: not in new data source
                    }
                };
            }

            ;
            if (product.StandardsNumber != null)
            {
                productGroup.StandardsSpecifications = new List<StandardSpecificationViewModel>
                {
                    new StandardSpecificationViewModel
                    {
                        Label = product.StandardsNumber,
                        Value = string.Empty, // TODO: not in new data source
                    }
                };
            }
            
            products.Add(productGroup);
        }

        return products;
    }
}