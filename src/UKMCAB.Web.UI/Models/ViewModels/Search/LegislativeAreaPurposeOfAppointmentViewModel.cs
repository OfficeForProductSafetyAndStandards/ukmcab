using UKMCAB.Web.UI.Models.ViewModels.Search.Shared;

namespace UKMCAB.Web.UI.Models.ViewModels.Search;

public class LegislativeAreaPurposeOfAppointmentViewModel
{
    public Guid LegislativeAreaId { get; set; }
    public string Name { get; set; } 
    public List<StandardViewModel> PurposeOfAppointments { get; set; }
}