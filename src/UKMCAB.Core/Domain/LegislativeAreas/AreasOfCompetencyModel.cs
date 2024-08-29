namespace UKMCAB.Data.Models.LegislativeAreas
{
    public class AreasOfCompetencyModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<Guid> ProtectionAgainstRiskIds { get; set; }
    }
}