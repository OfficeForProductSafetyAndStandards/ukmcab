
namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class LegislativeAreaListItemViewModel : LegislativeAreaBaseViewModel
    {
        public ListItem? LegislativeArea { get; set; } = new();

        public string? PurposeOfAppointment { get; set; }

        public string? Category { get; set; }

        public string? SubCategory { get; set; }

        public string? Product { get; set; }

        public List<string>? Procedures { get; set; } = new();

        public List<string>? Categories { get; set; } = new();

        public int NoOfProductsInScopeOfAppointment { get; set; }
        public int NoOfDesignatedStandardsInScopeOfAppointment  => DesignatedStandards?.Count ?? 0;
        public List<DesignatedStandardReadOnlyViewModel>? DesignatedStandards { get; set; }

        public LegislativeAreaListItemViewModel() : base("Legislative area list item") { }
        public LegislativeAreaListItemViewModel(
            Guid legislativeAreaId, 
            string legislativeArea, 
            string? purposeOfAppointment, 
            string? category, 
            string? subCategory, 
            Guid scopeId,
            string? product,
            List<string>? procedures = null,
            List<DesignatedStandardReadOnlyViewModel>? designatedStandards = null) : base("Legislative area list item")
        {
            LegislativeArea = new ListItem { Id = legislativeAreaId, Title = legislativeArea };
            PurposeOfAppointment = purposeOfAppointment;
            Category = category;
            SubCategory = subCategory;
            ScopeId = scopeId;
            Product = product;
            Procedures = procedures;
            DesignatedStandards = designatedStandards;
        }
    }
}
