using System.Diagnostics.Eventing.Reader;

namespace UKMCAB.Web.UI.Models.ViewModels;

public class ProductViewModel
{
    public string Name { get; set; }
    public string Code { get; set; }
    public string Part { get; set; }
    public string Module { get; set; }
    public string Schedule { get; set; }
    public string StandardsSpecification { get; set; }

    public bool hideBorder => string.IsNullOrWhiteSpace(Code) && string.IsNullOrWhiteSpace(Part) && string.IsNullOrWhiteSpace(StandardsSpecification) && string.IsNullOrWhiteSpace(Module) && string.IsNullOrWhiteSpace(Schedule);

}