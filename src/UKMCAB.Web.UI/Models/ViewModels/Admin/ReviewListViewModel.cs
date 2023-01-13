using UKMCAB.Core.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin
{
    public class ReviewListViewModel : ILayoutModel
    {
        public List<Document> SubmittedCABs { get; set; }
        public string? Title => "Pending reviews";
    }
}
