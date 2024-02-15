namespace UKMCAB.Data.Models.LegislativeAreas
{
    public class ProductAndProcedures
    {
        public Guid? ProductId { get; set; }
        public List<Guid> ProcedureIds { get; set; } = new();
    }
}
