namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{
    public class CABHistoryViewModel : CreateEditCABViewModel
    {
        public DateTime? LastAuditLogHistoryDate { get; set; }
        public string? CABId { get; set; }
    }
}
