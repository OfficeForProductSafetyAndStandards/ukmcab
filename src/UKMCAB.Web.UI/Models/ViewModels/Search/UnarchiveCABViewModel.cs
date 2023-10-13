using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Search
{
    public class UnarchiveCABViewModel : ILayoutModel
    {
        public string? CABId { get; set; }
        public string? ReturnURL { get; set; }

        [Required(ErrorMessage = "State the reason for unarchiving this CAB record")]
        public string? UnarchiveReason { get; set; }

        public string? Title => "Unarchive CAB profile";
    }
}
