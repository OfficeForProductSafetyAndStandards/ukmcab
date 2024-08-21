using Polly.Caching;

namespace UKMCAB.Data.Pagination
{
    public class PaginationInfo
    {
        public int PageIndex { get; private set; }
        public int PageNumber { get; private set; }
        public int PageSize { get; private set; }
        public int QueryCount { get; private set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageCount > PageNumber;
        public int PageCount =>
            QueryCount % PageSize == 0
                ? QueryCount / PageSize
                : QueryCount / PageSize + 1;
        public int Skip => PageIndex * PageSize;
        public int Take => (!HasNextPage && QueryCount % PageSize != 0)
                    ? QueryCount % PageSize
                    : PageSize;
        public int MaxPageRange { get; set; } = 5;
        public int FirstPageItemNo => QueryCount > 0 ? (PageNumber - 1) * PageSize + 1 : QueryCount;
        public int LastPageItemNo => PageNumber * PageSize < QueryCount
            ? PageNumber * PageSize
            : QueryCount;
        public List<int> PageRange
        {
            get
            {
                MaxPageRange = MaxPageRange < 3 ? 3 : MaxPageRange;

                var pageList = new List<int>();
                if (QueryCount == 0) return pageList;
                if (PageCount < (MaxPageRange + 1)) return Enumerable.Range(1, PageCount).ToList();
                if (PageNumber < (MaxPageRange - 1)) return Enumerable.Range(1, MaxPageRange).ToList();


                if (PageNumber > PageCount - 2)
                {
                    pageList = Enumerable.Range(PageCount - (MaxPageRange - 1), MaxPageRange).ToList();
                }
                else
                {
                    pageList = Enumerable.Range((PageNumber - 2) > 0 ? (PageNumber - 2) : 1, MaxPageRange).ToList();
                }

                if (!pageList.Contains(1))
                {
                    pageList.Insert(0, 1);
                }

                return pageList;
            }
        }

        public PaginationInfo(int pageNumber, int queryCount, int pageSize = 20)
        {
            PageIndex = pageNumber - 1;
            PageNumber = pageNumber;
            PageSize = pageSize;
            QueryCount = queryCount;
        }

        
    }
}
