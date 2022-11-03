using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels
{
    public class SearchViewModel
    {
        [Required]
        public string Keywords { get; set; }
    }
}
