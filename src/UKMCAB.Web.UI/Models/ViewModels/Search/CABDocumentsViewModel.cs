using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Search
{
    public class CABDocumentsViewModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string CABId { get; set; }
        public string DocumentType { get; set; }
        public List<FileUpload> Documents { get; set; }
    }
}
