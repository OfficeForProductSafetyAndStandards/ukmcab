namespace UKMCAB.Core.Domain.LegislativeAreas
{
    public class ProcedureModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid LegislativeAreaId { get; set; }

        public List<Guid> PurposeOfAppointmentIds { get; set; }

        public List<Guid> CategoryIds { get; set; }

        public List<Guid> ProductIds { get; set; }
    }
}
