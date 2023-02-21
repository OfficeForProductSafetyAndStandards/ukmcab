using System.Web;
using Humanizer;
using Microsoft.Extensions.Primitives;

namespace UKMCAB.Web.UI.Models.ViewModels.Search
{
    public class PaginationViewModel
    {
        public int PageNumber { get; set; }
        public int Total { get; set; }


        public int TotalPages => Total % Constants.SearchResultPerPage == 0
            ? Total / Constants.SearchResultPerPage
            : Total / Constants.SearchResultPerPage + 1;

        public bool ShowPrevious => PageNumber > 1;
        public bool ShowNext => PageNumber < TotalPages;
        public int FirstResult => Total > 0 ? (PageNumber - 1) * Constants.SearchResultPerPage + 1 : Total;
        public int LastResult => PageNumber * Constants.SearchResultPerPage < Total
            ? PageNumber * Constants.SearchResultPerPage
            : Total;

        public List<int> PageRange()
        {
            if (Total == 0) return new List<int>();
            if (TotalPages < 6) return Enumerable.Range(1, TotalPages).ToList();
            if (PageNumber < 4) return Enumerable.Range(1, 5).ToList();
            if (PageNumber > TotalPages - 2) return Enumerable.Range(TotalPages - 4, 5).ToList();
            return Enumerable.Range(PageNumber - 2, 5).ToList();
        }

        public string BaseURL(HttpContext context)
        {
            var queryItems = context.Request.QueryString;
            var collection = HttpUtility.ParseQueryString(queryItems.Value);
            collection.Remove("PageNumber");
            return context.Request.Path + "?" + collection.ToString();
        }
    }
}
