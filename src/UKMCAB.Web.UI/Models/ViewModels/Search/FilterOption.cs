namespace UKMCAB.Web.UI.Models.ViewModels.Search
{
    public class FilterOption
    {
        public FilterOption(string prefix, string filterValue, bool isSelected)
        {
            Id = $"{prefix}-{SanitiseIdString(filterValue)}";
            Value = filterValue;
            Selected = isSelected;
        }
        private static string SanitiseIdString(string id)
        {
            return id
                .ToLower()
                .Replace(":", string.Empty)
                .Replace(" ", string.Empty);
        }

        public string Id { get; }
        public string Value { get; }
        public bool Selected { get; set; }
    }
}
