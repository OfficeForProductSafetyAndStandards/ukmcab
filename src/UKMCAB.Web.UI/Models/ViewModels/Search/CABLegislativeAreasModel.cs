using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;

namespace UKMCAB.Web.UI.Models.ViewModels.Search;

public class CABLegislativeAreasModel
{
    public List<LegislativeAreaViewModel> ActiveLegislativeAreas { get; set; } = new();
    public List<LegislativeAreaViewModel> ArchivedLegislativeAreas { get; set; } = new();
    public string CabUrl { get; set; } = string.Empty;
    public Guid LegislativeAreaId { get; set; }
    public string? LegislativeAreaName { get; set; }

    public bool ShowProvisionalTag { get; set; }
    public bool ShowArchivedTag { get; set; }
    public string? Regulation { get; set; }
    public Guid? ScopeOfAppointmentId { get; set; }
    public List<(Guid Id, string Name)> PurposeOfAppointments { get; set; } = new();
    public (Guid? Id, string? Name) PurposeOfAppointment { get; set; }
    public (Guid? Id, string? Name) PpeProductType { get; set; }
    public (Guid? Id, Guid? SOAId, string? Name) ProtectionAgainstRisk { get; set; }
    public (Guid? Id, string? Name) Category { get; set; }
    public (Guid? Id, string? Name) SubCategory { get; set; }
    public (Guid? Id, string? Name) Product { get; set; }
    public (Guid? Id, Guid? SoaId, string? Name) AreaOfCompetency { get; set; }
    public List<(Guid Id, string Name)> Categories { get; set; } = new();
    public List<(Guid Id, string Name)> SubCategories { get; set; } = new();
    public List<(Guid Id, string Name)> Products { get; set; } = new();
    public List<(Guid Id, Guid? SoaId, string Name)> PpeProductTypes { get; set; } = new();
    public List<(Guid Id, Guid? SoaId, string Name)> ProtectionAgainstRisks { get; set; } = new();
    public List<(Guid Id, Guid SoaId, string Name)> AreaOfCompetencies { get; set; } = new();
    public List<(Guid Id, string Name)> Procedures { get; set; } = new();
    public List<DesignatedStandardReadOnlyViewModel> DesignatedStandards { get; set; } = new();
    public bool ShowLabels { get; set; } = true;
}