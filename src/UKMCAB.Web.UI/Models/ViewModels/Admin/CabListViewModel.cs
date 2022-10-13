namespace UKMCAB.Web.UI.Models.ViewModels.Admin
{
    public class CabListViewModel
    {
        public int PageNumber { get; set; }
        public List<CabListItemViewModel> CabListItems { get; set; }
    }

    public class CabListItemViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
    }
}
