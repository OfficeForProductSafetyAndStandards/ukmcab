using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Search
{
    public class ArchiveCABViewModel : ILayoutModel
    {
        public string? CABId { get; set; }
        public string? Name { get; set; }
        public string? ReturnURL { get; set; }

        [Required(ErrorMessage = "Enter notes")]
        [MaxLength(1000, ErrorMessage = "Maximum note length is 1000 characters")]
        public string? ArchiveInternalReason { get; set; }
        [MaxLength(1000, ErrorMessage = "Maximum reason length is 1000 characters")]
        public string? ArchivePublicReason { get; set; }
        public bool HasDraft { get; set; }

        public string? Title => "Archive CAB profile";
    }
}
