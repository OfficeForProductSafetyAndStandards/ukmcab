using Microsoft.AspNetCore.Mvc.Rendering;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Enums;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{
    public class ScheduleFileListViewModel : CreateEditCABViewModel, ILayoutModel
    {
        public string? Title { get; set; }
        public string? CABId { get; set; }
        public List<FileViewModel> ArchivedFiles { get; set; } = new();
        public List<FileViewModel> ActiveFiles { get; set; } = new();
        public bool ShowBanner { get; set; }
        public string? SuccessBannerTitle { get; set; }
        public IEnumerable<SelectListItem> LegislativeAreas { get; set; } = new List<SelectListItem>();
        public RemoveActionEnum RemoveAction { get; set; }     
        public bool ShowArchiveAction { get; set; }
        public string? SelectedArchivedScheduleId { get; set; }

        public string? SelectedScheduleId { get; set; }
    }
}
