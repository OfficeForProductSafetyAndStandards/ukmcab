namespace UKMCAB.Data.Models.LegislativeAreas
{
    public class Product
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid? LegislativeAreaId { get; set; }

        public Guid? PurposeOfAppointmentId { get; set; }

        public Guid? CategoryId { get; set; }
        public Guid? SubCategoryId { get; set; }
    }
}
