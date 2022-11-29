using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels
{
    public class SearchViewModel : ILayoutModel
    {
        [Required]
        public string Keywords { get; set; }

        string? ILayoutModel.Title => "Search";
        public bool IsAdmin { get; set; }
    }
}
