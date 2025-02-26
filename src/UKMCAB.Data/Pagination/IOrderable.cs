
namespace UKMCAB.Data.Pagination
{
    public interface IOrderable
    {
        public string Name { get; set; }
        public List<string> ReferenceNumber { get; set; }
    }
}
