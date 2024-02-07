namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Shared;

public class HelperDate
{
    public string? DateDay { get; set; }
    public string? DateMonth { get; set; }
    public string? DateYear { get; set; }
    public string Date => $"{DateDay}/{DateMonth}/{DateYear}";
}