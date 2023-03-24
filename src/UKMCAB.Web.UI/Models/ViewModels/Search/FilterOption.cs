using System.Globalization;

namespace UKMCAB.Web.UI.Models.ViewModels.Search
{
    public class FilterOption
    {
        public FilterOption(string prefix, string filterValue, bool isSelected)
        {
            Id = $"{prefix}-{SanitiseIdString(filterValue)}";
            Value = filterValue;
            Selected = isSelected;
            Label = prefix.Equals("bodytypes", StringComparison.CurrentCultureIgnoreCase) ? SanitiseLabel(filterValue) : filterValue;
        }
        private static string SanitiseIdString(string id)
        {
            return id
                .ToLower()
                .Replace(":", string.Empty)
                .Replace(" ", string.Empty);
        }

        private static string SanitiseLabel(string label)
        {
            switch (label)
            {
                case "third-country-body":
                    return "Third country body";
                case "Overseas Body":
                    return "Overseas body";
                default:
                    return label;
            }
        }


        public string Id { get; }
        public string Value { get; }
        public string Label { get; }
        public bool Selected { get; set; }
    }
}
