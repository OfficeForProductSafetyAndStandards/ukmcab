
using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Account
{
    public class RejectRequestViewModel : ILayoutModel
    {
        public string? AccountId { get; set; }

        [Required(ErrorMessage = "Enter a reason")]
        public string? Reason { get; set; }
        public string? Title => "Reject account request";
    }
}
