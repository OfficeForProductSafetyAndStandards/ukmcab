namespace UKMCAB.Data.Models.LegislativeAreas
{
    public class PpeProductType : IEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid PpeCategoryId { get; set; }
        public Guid LegislativeAreaId { get; set; }
    }
}