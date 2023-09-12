using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Search
{
    public class ArchiveCABViewModel : ILayoutModel
    {
        public string? CABId { get; set; }
        public string? ReturnURL { get; set; }

        [Required(ErrorMessage = "State the reason for archiving this CAB record")]
        public string? ArchiveReason { get; set; }
        public bool HasDraft { get; set; }

        public string? Title => "Archive CAB profile";
    }
}
