using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Search
{
    public class CABDocumentsTableViewModel
    {
        public string CABId { get; set; }
        public string DocumentType { get; set; }
        public IEnumerable<IGrouping<string?, FileUpload>> GroupedDocuments { get; set; }
    }
}
