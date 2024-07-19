using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{
    public class CABHistoryViewModel : CreateEditCABViewModel
    {
        public CABHistoryViewModel(string? cABId, List<Audit> documentAuditLog, string? returnUrl)
        {
            LastAuditLogHistoryDate = Enumerable.MaxBy(documentAuditLog, u => u.DateTime)?.DateTime;
            CABId = cABId;
            ReturnUrl = returnUrl;
        }
        public DateTime? LastAuditLogHistoryDate { get; set; }
        public string? CABId { get; set; }
    }
}
