namespace UKMCAB.Core.Domain.LegislativeAreas
{
    public class LegislativeAreaModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Regulation { get; set; }

        public bool HasDataModel { get; set; }
    }
}
