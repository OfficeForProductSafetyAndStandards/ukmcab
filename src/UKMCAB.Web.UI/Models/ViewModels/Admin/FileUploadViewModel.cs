using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin
{
    public class FileUploadViewModel : CreateEditCABViewModel, ILayoutModel 
    {
        public string? Title { get; set; }
        public string? CABId { get; set; }
        public List<FileViewModel>? UploadedFiles { get; set; }
        public IFormFile? File { get; set; }
    }

    public class FileViewModel
    {
        public string FileName { get; set; }

        [Required(ErrorMessage = "Enter a title for the file")]
        public string? Label { get; set; }
        public string? LegislativeArea { get; set; }
    }

    public static class SchedulesOptions
    {
        public const string UploadTitle = "Upload the Schedule of Accreditation";
        public const string ListTitle = "Schedules of Accreditation uploaded";
        public static readonly string[] AcceptedFileExtensions = new[] { ".pdf" };
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
