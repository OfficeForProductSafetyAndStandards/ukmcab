using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin
{
    public class FileListViewModel: CreateEditCABViewModel, ILayoutModel
    {
        public string? Title { get; set; }
        public string? CABId { get; set; }
        public List<FileViewModel>? UploadedFiles { get; set; }
    }
}
