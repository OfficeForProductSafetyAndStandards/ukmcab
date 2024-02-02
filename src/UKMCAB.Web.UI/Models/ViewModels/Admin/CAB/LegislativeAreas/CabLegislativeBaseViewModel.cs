using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeAreas
{
    public class CabLegislativeAreaBaseViewModel : ILayoutModel
    {   
        public string? CABId { get; set; }

        public string? Title { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
