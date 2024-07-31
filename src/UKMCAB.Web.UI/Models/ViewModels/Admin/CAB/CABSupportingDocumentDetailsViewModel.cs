using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{
    public class CABSupportingDocumentDetailsViewModel : CreateEditCABViewModel
    {
        public CABSupportingDocumentDetailsViewModel() { }

        public CABSupportingDocumentDetailsViewModel(Document document)
        {
            CABId = document.CABId;
            Documents = document.Documents ?? new List<FileUpload>();            
            IsCompleted = this.SupportingDocumentsCompleted();
        }

        public List<FileUpload>? Documents { get; set; }

        //public bool IsCompleted { get; set; }

        public string? CABId { get; set; }

        public bool SupportingDocumentsCompleted()
        {
            if (this.Documents != null && this.Documents.Any())
            {
                // any file where file label is empty/null
                if (this.Documents.Any(u => string.IsNullOrWhiteSpace(u.Label)))
                {
                    return false;
                }
                // any file where category not selected
                else if (this.Documents.Any(u => string.IsNullOrWhiteSpace(u.Category)))
                {
                    return false;
                }
                // any duplicate file labels/category
                else if (this.Documents.Where(x => !string.IsNullOrWhiteSpace(x.Category) && !string.IsNullOrWhiteSpace(x.Label)).GroupBy(x => new { Label = x.Label!.ToLower(), Category = x.Category!.ToLower() }).Any(g => g.Count() > 1))
                {
                    return false;
                }
                // any duplicate file/category
                else if (this.Documents.Where(x => !string.IsNullOrWhiteSpace(x.Category)).GroupBy(x => new { FileName = x.FileName.ToLower(), Category = x.Category!.ToLower() }).Any(g => g.Count() > 1))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            return true;
        }
    }
}
