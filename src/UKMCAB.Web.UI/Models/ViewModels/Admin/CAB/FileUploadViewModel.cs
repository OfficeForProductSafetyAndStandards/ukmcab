using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{
    public class FileUploadViewModel : CreateEditCABViewModel, ILayoutModel 
    {
        public string? Title { get; set; }
        public string? CABId { get; set; }
        public List<FileViewModel>? UploadedFiles { get; set; }
        public IFormFile? File { get; set; }
        public List<IFormFile>? Files { get; set; }
        public string? FileToUseAgain { get; set; }
    }

    public class FileViewModel
    {
        public string FileName { get; set; }

        [Required(ErrorMessage = "Enter a title for the file")]
        public string? Label { get; set; }
        public string? LegislativeArea { get; set; }
        public string? Category { get; set; }
        public int FileIndex { get; set; }
        public bool IsSelected { get; set; } = false;
        public bool IsDuplicated { get; set; } = false;
    }

    public static class SchedulesOptions
    {
        public const string UploadTitle = "Product schedules upload";
        public const string ListTitle = "Product schedules uploaded";
        public const string UseFileAgainTitle = "Use file again";
        public static readonly string[] AcceptedFileExtensions = new[] { ".pdf" };
        public const int MaxFileCount = 35;
        public static readonly Dictionary<string, string> AcceptedFileExtensionsContentTypes = new()
        {
            {".pdf", "application/pdf"}
        };
        public const string AcceptedFileTypes =  "PDF" ;
    }

    public static class DocumentsOptions
    {
        public const string UploadTitle = "Upload the supporting documents";
        public const string ListTitle = "Supporting documents uploaded";
        public static readonly string[] AcceptedFileExtensions = new[] { ".doc", ".docx", ".xls", ".xlsx", ".pdf" };
        public const int MaxFileCount = 10;
        public static readonly Dictionary<string, string> AcceptedFileExtensionsContentTypes = new()
        {
            { ".doc", "application/msword" },
            { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            { ".pdf", "application/pdf" },
            { ".xls", "application/xml" },
            { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" }
        };
        public const string AcceptedFileTypes =  "Word, Excel or PDF";
    }
}
