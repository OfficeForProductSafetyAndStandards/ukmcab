namespace UKMCAB.Data.Models.LegislativeAreas
{
    public class PpeCategoryModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid LegislativeAreaId { get; set; }
    }
}