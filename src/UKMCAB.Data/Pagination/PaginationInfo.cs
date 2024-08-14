namespace UKMCAB.Data.Pagination
{
    public class PaginationInfo
    {
        public int PageIndex { get; private set; }
        public int PageSize { get; private set; }
        public int QueryCount { get; private set; }
        public int PageCount 
        { 
            get
            {
                if (PageSize >= QueryCount)
                {
                    return 1;
                }
                else
                {
                    return (QueryCount % PageSize == 0)
                        ? QueryCount / PageSize
                        : QueryCount / PageSize + 1;
                }
            } 
        }
        public int Skip => PageIndex * PageSize;
        public int Take
        {
            get
            {
                var hasNextPage = PageCount > (PageIndex + 1);
                var take = (!hasNextPage && QueryCount % PageSize != 0)
                    ? QueryCount % PageSize
                    : PageSize;
                return take;
            }
        }

        public PaginationInfo(int pageIndex, int queryCount, int pageSize = 20)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            QueryCount = queryCount;
        }
    }
}
