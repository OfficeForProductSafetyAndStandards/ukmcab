namespace UKMCAB.Data.Models.LegislativeAreas
{
    public class Procedure
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid LegislativeAreaId { get; set; }

        public List<Guid> PurposeOfAppointmentIds { get; set; }

        public List<Guid> CategoryIds { get; set; }
        public List<Guid> ProductIds { get; set; }
        public List<Guid> PpeProductTypeIds { get; set; }
        public List<Guid> ProtectionAgainstRiskIds { get; set; }
        public List<Guid> AreaOfCompetencyIds { get; set; }
    }
}
