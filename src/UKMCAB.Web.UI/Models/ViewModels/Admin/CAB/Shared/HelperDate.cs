namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Shared;

public class HelperDate
{
    public string? DateDay { get; init; }
    public string? DateMonth { get; init; }
    public string? DateYear { get; init; }
    public string Date => $"{DateDay}/{DateMonth}/{DateYear}";
}