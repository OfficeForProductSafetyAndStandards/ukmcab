using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{
    public class CABHistoryViewModel : CreateEditCABViewModel
    {
        public CABHistoryViewModel() { }
        public CABHistoryViewModel(string? cABId, List<Audit> documentAuditLog, string? returnUrl)
        {
            LastAuditLogHistoryDate = Enumerable.MaxBy(documentAuditLog, u => u.DateTime)?.DateTime;
            CABId = cABId;
            ReturnUrl = returnUrl;
        }
        public DateTime? LastAuditLogHistoryDate { get; private set; }
        public string? CABId { get; private set; }
    }
}
