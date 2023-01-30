namespace UKMCAB.Web.UI.Models.ViewModels.Admin
{
    public class FileUploadViewModel : ILayoutModel 
    {
        public string? Title { get; set; }
        public string? Id { get; set; }
        public List<string>? UploadedFiles { get; set; }
        public IFormFile? File { get; set; }
    }

    public static class SchedulesOptions
    {
        public const string UploadTitle = "Upload the Schedule of Accreditation";
        public const string ListTitle = "Schedules of Accreditation uploaded";
        public static readonly string[] AcceptedFileExtensions = new[] { ".pdf" };
        public const string AcceptedFileTypes =  "PDF" ;
    }

    public static class DocumentsOptions
    {
        public const string UploadTitle = "Upload the supporting documents";
        public const string ListTitle = "Supporting documents uploaded";
        public static readonly string[] AcceptedFileExtensions = new[] { ".doc", ".docx", ".xls", ".xlsx", ".pdf" };
        public const string AcceptedFileTypes =  "Word, Excel or PDF";
    }
}
