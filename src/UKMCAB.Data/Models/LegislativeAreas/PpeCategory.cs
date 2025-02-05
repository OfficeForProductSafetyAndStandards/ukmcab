namespace UKMCAB.Data.Models.LegislativeAreas
{
    public class PpeCategory
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid LegislativeAreaId { get; set; }
    }
}