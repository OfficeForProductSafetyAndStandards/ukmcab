using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Enums;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{
    public class FileListViewModel: CreateEditCABViewModel, ILayoutModel
    {
        public string? Title { get; set; }
        public string? SubTitle { get; set; }
        public string? CABId { get; set; }        
        public List<FileViewModel>? UploadedFiles { get; set; }
        public bool ShowBanner { get; set; }
        public string? SuccessBannerTitle { get; set; }
        public List<string> LegislativeAreas { get; set; } = new();
        public RemoveActionEnum RemoveAction { get; set; }       
    }
}
