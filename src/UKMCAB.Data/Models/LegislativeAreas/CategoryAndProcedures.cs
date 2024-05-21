namespace UKMCAB.Data.Models.LegislativeAreas
{
    public class CategoryAndProcedures
    {
        public Guid? CategoryId { get; set; }
        public List<Guid> CategoryIds { get; set; } = new();
    }
}
