using System.ComponentModel.DataAnnotations;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Enums;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{
    public class RemoveViewModel : ILayoutModel
    {
        [Required(ErrorMessage = "Select an option")]
        public RemoveActionEnum Action { get; set; }

        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }
}
