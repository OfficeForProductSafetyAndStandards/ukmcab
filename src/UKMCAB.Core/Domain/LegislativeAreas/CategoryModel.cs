namespace UKMCAB.Core.Domain.LegislativeAreas
{
    public class CategoryModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid LegislativeAreaId { get; set; }

        public Guid PurposeOfAppointmentId { get; set; }
    }
}
