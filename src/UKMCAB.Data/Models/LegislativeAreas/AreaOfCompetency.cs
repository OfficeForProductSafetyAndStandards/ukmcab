namespace UKMCAB.Data.Models.LegislativeAreas
{
    public class AreaOfCompetency
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<Guid> ProtectionAgainstRiskIds { get; set; }
    }
}