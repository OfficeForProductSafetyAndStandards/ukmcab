using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class LegislativeAreaBaseViewModel : ILayoutModel
    {
        public string? CABId { get; set; }

        public string? Title { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
