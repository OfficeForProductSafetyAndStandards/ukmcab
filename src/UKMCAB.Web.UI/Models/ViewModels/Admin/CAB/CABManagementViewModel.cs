using UKMCAB.Web.UI.Models.ViewModels.Shared;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{
    public class CABManagementViewModel : ILayoutModel
    {
        public string? Title => "Draft management";
        public string TabName { get; set; }
        public string SortField { get; set; }
        public string SortDirection { get; set; }
        public List<CABManagementItemViewModel>? CABManagementItems { get; set; }
        public PaginationViewModel? Pagination { get; set; }
        public string RoleId { get; set; } = string.Empty;

        public int AllCount { get; set; }
        public int DraftCount { get; set; }
        public int PendingDraftCount { get; set; }
        public int PendingPublishCount { get; set; }
        public int PendingArchiveCount { get; set; }
    }
}
