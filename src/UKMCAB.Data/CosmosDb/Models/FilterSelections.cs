namespace UKMCAB.Data.CosmosDb.Models
{
    public class FilterSelections
    {
        public string[] BodyTypes { get; set; }
        public string[] RegisteredOfficeLocations { get; set; }
        public string[] TestingLocations { get; set; }
        public string[] Regulations { get; set; }
    }
}
