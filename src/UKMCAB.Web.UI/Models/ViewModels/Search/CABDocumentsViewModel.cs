using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Models.ViewModels.Search.Enums;

namespace UKMCAB.Web.UI.Models.ViewModels.Search
{
    public class CABDocumentsViewModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string CABId { get; set; }
        public string DocumentType { get; set; }
        public ArchivedFilterOption? ProductScheduleFilter { get; set; }
        public List<FileUpload> Documents { get; set; }
    }
}
