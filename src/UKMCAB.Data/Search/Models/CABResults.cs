namespace UKMCAB.Data.Search.Models
{
    public class CABResults
    {
        public List<CABIndexItem> CABs { get; set; }
        public int PageNumber { get; set; }
        public int Total { get; set; }
    }
}
