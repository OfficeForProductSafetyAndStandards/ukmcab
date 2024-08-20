namespace UKMCAB.Web.UI.Models.ViewModels.Shared
{
    public class PaginationViewModel
    {
        public int PageNumber { get; set; }
        public int Total { get; set; }
        public int ResultsPerPage { get; set; } 
        public string ResultType { get; set; } 
        public string? TabId { get; set; }
        public int MaxPageRange { get; set; } = 5;
        public string? QueryString { get; set; }


        public int TotalPages => Total % ResultsPerPage == 0
            ? Total / ResultsPerPage
            : Total / ResultsPerPage + 1;

        public bool ShowPrevious => PageNumber > 1;
        public bool ShowNext => PageNumber < TotalPages;
        public int FirstResult => Total > 0 ? (PageNumber - 1) * ResultsPerPage + 1 : Total;
        public int LastResult => PageNumber * ResultsPerPage < Total
            ? PageNumber * ResultsPerPage
            : Total;

        public PaginationViewModel() 
        { 
        }

        public PaginationViewModel(int pageNumber, int total, int resultsPerPage, string? resultType = null)
        {
            PageNumber = pageNumber;
            Total = total;
            ResultsPerPage = resultsPerPage;
            ResultType = resultType ?? string.Empty;
        }

        public List<int> PageRange()
        {
            MaxPageRange = MaxPageRange < 3 ? 3 : MaxPageRange;

            var pageList = new List<int>();
            if (Total == 0) return pageList;
            if (TotalPages < (MaxPageRange+1)) return Enumerable.Range(1, TotalPages).ToList();
            if (PageNumber < (MaxPageRange-1)) return Enumerable.Range(1, MaxPageRange).ToList();


            if (PageNumber > TotalPages - 2)
            {
                pageList = Enumerable.Range(TotalPages - (MaxPageRange - 1), MaxPageRange).ToList();
            }
            else
            {
                pageList = Enumerable.Range((PageNumber - 2) > 0 ? (PageNumber-2): 1, MaxPageRange).ToList();
            }

            if (!pageList.Contains(1))
            {
                pageList.Insert(0, 1);
            }

            return pageList;
        }
    }
}
