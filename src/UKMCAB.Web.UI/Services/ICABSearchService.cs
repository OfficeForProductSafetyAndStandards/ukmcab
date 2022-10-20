using UKMCAB.Web.UI.Models.ViewModels;

namespace UKMCAB.Web.UI.Services;

public interface ICABSearchService
{
    CABProfileViewModel GetCAB(string id);
}