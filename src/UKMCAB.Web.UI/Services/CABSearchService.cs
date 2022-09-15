using UKMCAB.Data;
using UKMCAB.Web.UI.Models.ViewModels;

namespace UKMCAB.Web.UI.Services;

public class CABSearchService : ICABSearchService
{
    public List<SearchResultViewModel> Search(string text, string[] bodyTypes, string[] registeredOfficeLocations, string[] testingLocations, string[] legislativeAreas)
    {
        var cabData = CabRepository.Search(text, bodyTypes,
            registeredOfficeLocations, testingLocations,
            legislativeAreas);

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

    public CABProfileViewModel GetCAB(string id)
    {
        var cabData = CabRepository.Get(id);
        
        var cabProfile = new CABProfileViewModel
        {
            Name = cabData.Name,
            
            Published = "TBD", // todo
            LastUpdated = "TBD", // todo
            
            Address = cabData.Address,
            Website = cabData.Website,
            Email = cabData.Email,
            Phone = cabData.Phone,

            BodyNumber = cabData.BodyNumber,
            BodyType = string.Join(", ", cabData.BodyType),
            RegisteredOfficeLocation = string.Join(", ", cabData.RegisteredOfficeLocation),
            TestingLocations = string.Join(", ", cabData.TestingLocations),
            AccreditaionBody = $"{cabData.AccreditationBodyName}<br />{cabData.AccreditationBodyAddress}",
            AccreditationStandard = cabData.AccreditationStandard,
            AppointmentDetails = cabData.AppointmentDetails,
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
        cabProfile.CertificationActivityLocations = cabData.CertificationActivitiesLocations != null
            ? cabData.CertificationActivitiesLocations.Select(c => c.Line).ToList()
            : new List<string>();


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
                Description = cabDataRegulation.Description
            };
            regulation.ProductGroups = cabDataRegulation.ProductGroups != null
                ? GetProductGroups(cabDataRegulation.ProductGroups)
                : new List<ProductsGroupViewModel>();
            
            regulations.Add(regulation);
        }

        return regulations;
    }

    private List<ProductsGroupViewModel> GetProductGroups(List<ProductGroup> productGroups)
    {
        var products = new List<ProductsGroupViewModel>();
        foreach (var regulationProductGroup in productGroups)
        {
            var productGroup = new ProductsGroupViewModel()
            {
                Title = regulationProductGroup.Name,
                Description = regulationProductGroup.Description,
                Products = regulationProductGroup.Lines.Select(l => l.Name).ToList(),
                Schedules = new List<ScheduleViewModel>()
            };
            if (regulationProductGroup.Schedules != null)
            {
                productGroup.Schedules = regulationProductGroup.Schedules.Select(schedule => new ScheduleViewModel
                {
                    Title = schedule.Name,
                    Description = schedule.Description,
                    Label = schedule.Label
                }).ToList();
            }
            
            products.Add(productGroup);
        }

        return products;
    }
}