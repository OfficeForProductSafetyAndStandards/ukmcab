namespace UKMCAB.Core.Domain.LegislativeAreas
{
    public class PurposeOfAppointmentModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid LegislativeAreaId { get; set; }
    }
}
