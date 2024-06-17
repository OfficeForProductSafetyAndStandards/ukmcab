using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Enums;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{
    public class LegislativeAreaRemoveViewModel : ILayoutModel
    {
        public RemoveActionEnum? LegislativeAreaRemoveAction { get; set; }

        public Guid CabId { get; set; }

        public CABLegislativeAreasItemViewModel LegislativeArea { get; set; } = new();

        public string Title { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }

        public bool FromSummary { get; set; }

        public string? UserRoleId { get; set; }
    }
}
