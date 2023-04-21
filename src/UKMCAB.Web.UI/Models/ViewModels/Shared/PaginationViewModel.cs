using System.Web;
using UKMCAB.Data;

namespace UKMCAB.Web.UI.Models.ViewModels.Shared
{
    public class PaginationViewModel
    {
        public int PageNumber { get; set; }
        public int Total { get; set; }
        public int ResultsPerPage { get; set; } 
        public string ResultType { get; set; } 


        public int TotalPages => Total % ResultsPerPage == 0
            ? Total / ResultsPerPage
            : Total / ResultsPerPage + 1;

        public bool ShowPrevious => PageNumber > 1;
        public bool ShowNext => PageNumber < TotalPages;
        public int FirstResult => Total > 0 ? (PageNumber - 1) * ResultsPerPage + 1 : Total;
        public int LastResult => PageNumber * ResultsPerPage < Total
            ? PageNumber * ResultsPerPage
            : Total;

        public List<int> PageRange()
        {
            if (Total == 0) return new List<int>();
            if (TotalPages < 6) return Enumerable.Range(1, TotalPages).ToList();
            if (PageNumber < 4) return Enumerable.Range(1, 5).ToList();
            if (PageNumber > TotalPages - 2) return Enumerable.Range(TotalPages - 4, 5).ToList();
            return Enumerable.Range(PageNumber - 2, 5).ToList();
        }
    }
}
