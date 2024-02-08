namespace UKMCAB.Core.Domain.LegislativeAreas
{
    public class SubCategoryModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid? CategoryId { get; set; }

    }
}
