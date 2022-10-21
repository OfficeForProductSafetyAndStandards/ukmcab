using UKMCAB.Data.CosmosDb.Models;
using UKMCAB.Web.UI.Models;
using UKMCAB.Web.UI.Models.ViewModels;

namespace UKMCAB.Web.UI.Services;

public interface ICABSearchService
{
    Task<List<CAB>> SearchCABsAsync(string text, FilterSelections filterSelections);
    Task<CABProfileViewModel> GetCABAsync(string id);
}