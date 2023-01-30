namespace UKMCAB.Web.UI.Models.ViewModels.Admin
{
    public class FileListViewModel: ILayoutModel
    {
        public string? Title { get; set; }
        public string? Id { get; set; }
        public List<string>? UploadedFiles { get; set; }
    }
}
