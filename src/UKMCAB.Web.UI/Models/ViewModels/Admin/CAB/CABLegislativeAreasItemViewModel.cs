using UKMCAB.Common.Extensions;
using UKMCAB.Core.Security;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{
    public class CABLegislativeAreasItemViewModel
    {
        public Guid? LegislativeAreaId { get; set; }
        
        public string? Name { get; set; }

        public bool? IsProvisional { get; set; }

        public DateTime? AppointmentDate { get; set; }

        public DateTime? ReviewDate { get; set; }

        public string? Reason { get; set; }

        public string? PointOfContactName { get; set; }
        public string? PointOfContactEmail { get; set; }
        public string? PointOfContactPhone { get; set; }
        public bool? IsPointOfContactPublicDisplay { get; set; }

        public List<LegislativeAreaListItemViewModel> ScopeOfAppointments { get; set; } = new();

        public bool CanChooseScopeOfAppointment { get; set; }
        public bool? IsArchived { get; init; }
        public Guid? SelectedScopeofAppointmentId { get; set; }
        public bool ShowPurposeOfAppointmentColumn => ScopeOfAppointments != null && ScopeOfAppointments.Any(x => !string.IsNullOrEmpty(x.PurposeOfAppointment));
        public bool ShowCategoryColumn => ScopeOfAppointments != null && ScopeOfAppointments.Any(x => !string.IsNullOrEmpty(x.Category));
        public bool ShowProductColumn => ScopeOfAppointments != null && ScopeOfAppointments.Any(x => !string.IsNullOrEmpty(x.Product));
        public LAStatus Status { get; set; }
        public string StatusName => Status.GetEnumDescription();
    }
}
