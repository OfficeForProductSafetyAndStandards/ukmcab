namespace UKMCAB.Data.Search.Models
{
    public class CABSearchOptions
    {
        public int PageNumber { get; set; }
        public string Keywords { get; set; }

        public string Sort { get; set; }

        public string[] BodyTypesFilter { get; set; }
        public string[] LegislativeAreasFilter { get; set; }
        public string[] RegisteredOfficeLocationsFilter { get; set; }
        public string[] StatusesFilter { get; set; }
        public string[] UserGroupsFilter { get; set; }

        public bool IgnorePaging { get; set; }
        public bool InternalSearch { get; set; }

        public List<string> Select { get; set; } = new List<string>();
    }
}
