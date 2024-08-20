using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;
using UKMCAB.Data.Models;
using System.Security.Claims;

namespace UKMCAB.Web.UI.Services
{
    public interface ILegislativeAreaDetailService
    {
        Task<CABLegislativeAreasItemViewModel> PopulateCABLegislativeAreasItemViewModelAsync(Document? cab,
        Guid LegislativeAreaId);
        List<DocumentLegislativeArea> GetPendingApprovalDocumentLegislativeAreaList(Document document, ClaimsPrincipal user);
    }
}
