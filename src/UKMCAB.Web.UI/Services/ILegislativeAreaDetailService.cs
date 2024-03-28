using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;
using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Services
{
    public interface ILegislativeAreaDetailService
    {
        Task<CABLegislativeAreasItemViewModel> PopulateCABLegislativeAreasItemViewModelAsync(Document? cab,
        Guid LegislativeAreaId);
    }
}
