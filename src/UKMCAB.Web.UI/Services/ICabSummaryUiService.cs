using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;

namespace UKMCAB.Web.UI.Services
{
    public interface ICabSummaryUiService
    {
        Task CreateDocumentAsync(Document document, bool? revealEditActions);
        string? GetSuccessBannerMessage();
        Task LockCabForUser(CABSummaryViewModel model);
    }
}
