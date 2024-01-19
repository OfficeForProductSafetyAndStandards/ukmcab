namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{
    public class FileListViewModel: CreateEditCABViewModel, ILayoutModel
    {
        public string? Title { get; set; }
        public string? CABId { get; set; }        
        public List<FileViewModel>? UploadedFiles { get; set; }
        public bool ShowBanner { get; set; }
        public string? SuccessBannerTitle { get; set; }
    }
}
