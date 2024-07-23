using UKMCAB.Core.Extensions;
using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{
    public class CABHistoryViewModel : CreateEditCABViewModel
    {
        public CABHistoryViewModel() { }

        public CABHistoryViewModel(Document document, string? returnUrl)
        {
            LastAuditLogHistoryDate = document.LastAuditLogHistoryDate();
            CABId = document.CABId;
            ReturnUrl = returnUrl;
        }
        public DateTime? LastAuditLogHistoryDate { get; private set; }
        public string? CABId { get; private set; }
    }
}
