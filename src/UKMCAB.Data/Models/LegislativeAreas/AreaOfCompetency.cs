namespace UKMCAB.Data.Models.LegislativeAreas
{
    public class AreaOfCompetency : IEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<Guid> ProtectionAgainstRiskIds { get; set; }
    }
}