namespace UKMCAB.Data.Search.Models
{
    public class CABResults
    {
        public List<CABDocument> CABs { get; set; }
        public int PageNumber { get; set; }
        public int Total { get; set; }
    }
}
