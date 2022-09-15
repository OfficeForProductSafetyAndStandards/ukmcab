namespace UKMCAB.Web.UI.Models.ViewModels;

public class RegulationViewModel
{
    public string Title { get; set; }
    public string Description { get; set; }
    public List<ProductsGroupViewModel> ProductGroups { get; set; }
}