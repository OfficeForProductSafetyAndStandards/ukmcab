
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
        public string? PpeProductType { get; set; }
        public string? ProtectionAgainstRisk { get; set; }
        public string? AreaOfCompetency { get; set; }
        public List<string>? Procedures { get; set; } = new();
        public List<string>? Categories { get; set; } = new();
        public int NoOfProductsInScopeOfAppointment { get; set; }
        public int NoOfDesignatedStandardsInScopeOfAppointment  => DesignatedStandards?.Count ?? 0;
        public List<DesignatedStandardReadOnlyViewModel>? DesignatedStandards { get; set; }

        public string RadioDescription
        {
            get
            {
                var description = new List<string>();

                if (!string.IsNullOrEmpty(Title))
                    description.Add($"Title: {Title}");

                if (!string.IsNullOrEmpty(PurposeOfAppointment))
                    description.Add($"Purpose of appointment: {PurposeOfAppointment}");

                if (!string.IsNullOrEmpty(Category))
                    description.Add($"Category: {Category}");
                if (!string.IsNullOrEmpty(SubCategory))
                    description.Add($"Sub-category: {SubCategory}");

                if (!string.IsNullOrEmpty(Product))
                    description.Add($"Product: {Product}");


                return string.Join(", ", description);
            }
        }

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

        public LegislativeAreaListItemViewModel(
            Guid legislativeAreaId,
            string legislativeArea,
            string? purposeOfAppointment,
            string? category,
            string? subCategory,
            Guid scopeId,
            string? product,
            string? ppeProductTypeName,
            string? protectionAgainstRiskName,
            string? areaOfCompetencyName,
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
            PpeProductType = ppeProductTypeName;
            ProtectionAgainstRisk = protectionAgainstRiskName;
            AreaOfCompetency = areaOfCompetencyName;
        }
    }
}
