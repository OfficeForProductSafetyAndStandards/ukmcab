using UKMCAB.Web.UI.Models.ViewModels;

namespace UKMCAB.Web.UI.Services;

public interface ICABSearchService
{
    List<SearchResultViewModel> Search(string text, string[] bodyTypes, string[] registeredOfficeLocations, string[] testingLocations, string[] regulations);

    CABProfileViewModel GetCAB(string id);
}