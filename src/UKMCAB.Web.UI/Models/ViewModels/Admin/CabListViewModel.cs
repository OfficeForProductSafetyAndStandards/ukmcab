namespace UKMCAB.Web.UI.Models.ViewModels.Admin
{
    public class CabListViewModel
    {
        public PaginationViewModel Pagination { get; set; }
        public List<CabListItemViewModel> CabListItems { get; set; }
    }

    public class CabListItemViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
    }

    public class PaginationViewModel
    {
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public bool ShowPrevious => PageNumber > 1;
        public bool ShowNext => PageNumber < TotalPages;
        public bool ShowPagination => TotalPages > 1;
    }
}
