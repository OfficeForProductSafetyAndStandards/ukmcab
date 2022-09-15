namespace UKMCAB.Web.UI.Models.ViewModels;

public class ProductsGroupViewModel
{
    public string Title { get; set; }
    public string Description { get; set; }
    public List<string> Products { get; set; }
    public List<ScheduleViewModel> Schedules { get; set; }
    public List<StandardSpecificationViewModel> StandardsSpecifications { get; set; }

}