using System.ComponentModel.DataAnnotations;
using UKMCAB.Data.Models.LegislativeAreas;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class LegislativeAreaListItemViewModel : LegislativeAreaBaseViewModel
    {
        public ListItem? LegislativeArea { get; set; } = new();

        public string? PurposeOfAppointment { get; set; }

        public string? Category { get; set; }

        public string? SubCategory { get; set; }

        public string? Product { get; set; }

        public List<string> Procedures { get; set; } = new();

        public List<string>? Categories { get; set; } = new();

        public int NoOfProductsInScopeOfAppointment { get; set; }

        [Obsolete("This constructor is obsolete. Use LegislativeAreaListItemViewModel(Guid legislativeAreaId, string legislativeArea, string? purposeOfAppointment, string? category,  string? subCategory, Guid scopeId, string? product, List<string> procedures.")]
        public LegislativeAreaListItemViewModel() { }

        public LegislativeAreaListItemViewModel(
            Guid legislativeAreaId, 
            string legislativeArea, 
            string? purposeOfAppointment, 
            string? category, 
            string? subCategory, 
            Guid scopeId,
            string? product,
            List<string> procedures) 
        {
            LegislativeArea = new ListItem { Id = legislativeAreaId, Title = legislativeArea };
            PurposeOfAppointment = purposeOfAppointment;
            Category = category;
            SubCategory = subCategory;
            ScopeId = scopeId;
            Product = product;
            Procedures = procedures;
        }
    }
}
