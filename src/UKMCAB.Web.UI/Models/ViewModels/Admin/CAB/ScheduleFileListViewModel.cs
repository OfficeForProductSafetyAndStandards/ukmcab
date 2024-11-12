using Microsoft.AspNetCore.Mvc.Rendering;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Enums;
using UKMCAB.Core.Security;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{
    public class ScheduleFileListViewModel : CreateEditCABViewModel, ILayoutModel
    {
        public string? Title { get; set; }
        public string? SubTitle { get; set; }
        public string? CABId { get; set; }
        public List<FileViewModel> ArchivedFiles { get; set; } = new();
        public List<FileViewModel> ActiveFiles { get; set; } = new();        
        public string? SuccessBannerTitle { get; set; }
        public IEnumerable<SelectListItem> LegislativeAreas { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> CreatedBy { get; set; } = new List<SelectListItem>
        {
            new()
            {
                Text = "Select",
                Value = string.Empty
            },
            new()
            {
                Text = Roles.OPSS.Label,
                Value = Roles.OPSS.Id
            },
            new()
            {
                Text = Roles.UKAS.Label,
                Value = Roles.UKAS.Id
            }
        };
        public RemoveActionEnum RemoveAction { get; set; }     
        public bool ShowArchiveAction { get; set; }
        public string? SelectedArchivedScheduleId { get; set; }

        public string? SelectedScheduleId { get; set; }
    }
}
