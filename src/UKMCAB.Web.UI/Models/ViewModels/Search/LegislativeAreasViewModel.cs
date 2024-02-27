namespace UKMCAB.Web.UI.Models.ViewModels.Search;

public class LegislativeAreaViewModel
{
    public Guid LegislativeAreaId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Regulation { get; set; }
    public bool IsProvisional { get; set; }
    public bool IsArchived { get; set; }
}