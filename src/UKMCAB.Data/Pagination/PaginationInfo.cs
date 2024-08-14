namespace UKMCAB.Data.Pagination
{
    public class PaginationInfo
    {
        public int PageIndex { get; private set; }
        public int PageSize { get; private set; }
        public int ResultsCount { get; private set; }
        public int PageCount 
        { 
            get
            {
                if (PageSize >= ResultsCount)
                {
                    return 1;
                }
                else
                {
                    return (ResultsCount % PageSize == 0)
                        ? ResultsCount / PageSize
                        : ResultsCount / PageSize + 1;
                }
            } 
        }

        public PaginationInfo(int pageIndex, int resultsCount, int pageSize = 20)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            ResultsCount = resultsCount;
        }

        public (int Skip, int Take) CalculateSkipAndTake()
        { 
            var hasNextPage = PageCount > (PageIndex + 1);
            var skip = PageIndex * PageSize;
            var take = (!hasNextPage && ResultsCount % PageSize != 0)
                ? ResultsCount % PageSize
                : PageSize;
            return (skip, take);
        }
    }
}
