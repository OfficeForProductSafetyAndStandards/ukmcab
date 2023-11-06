using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Search
{
    public class ArchiveCABViewModel : ILayoutModel
    {
        public string? CABId { get; set; }
        public string? Name { get; set; }
        public string? ReturnURL { get; set; }

        [Required(ErrorMessage = "Enter notes")]
        public string? ArchiveInternalReason { get; set; }
        public string? ArchivePublicReason { get; set; }
        public bool HasDraft { get; set; }

        public string? Title => "Archive CAB profile";
    }
}
