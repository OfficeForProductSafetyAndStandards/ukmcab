namespace UKMCAB.Data.Pagination
{
    public class PaginationInfo
    {
        public int PageIndex { get; private set; }
        public int PageNumber { get; private set; }
        public int PageSize { get; private set; }
        public int QueryCount { get; private set; }
        public int PageCount =>
            QueryCount % PageSize == 0
                ? QueryCount / PageSize
                : QueryCount / PageSize + 1;
        public int Skip => PageIndex * PageSize;
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageCount > PageNumber;
        public int Take
        {
            get
            {
                var take = (!HasNextPage && QueryCount % PageSize != 0)
                    ? QueryCount % PageSize
                    : PageSize;
                return take;
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
