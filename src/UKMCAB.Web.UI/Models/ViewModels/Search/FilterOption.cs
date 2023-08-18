using System.Globalization;
using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Search
{
    public class FilterOption
    {
        public FilterOption(string prefix, string filterValue, bool isSelected)
        {
            Id = $"{prefix}-{SanitiseIdString(filterValue)}";
            Value = filterValue;
            Selected = isSelected;
            Label = SanitiseLabel(prefix, filterValue);
        }
        private static string SanitiseIdString(string id)
        {
            return id
                .ToLower()
                .Replace(":", string.Empty)
                .Replace(" ", string.Empty);
        }

        private static string SanitiseLabel(string prefix, string label)
        {
            if (prefix.Equals("bodytypes", StringComparison.CurrentCultureIgnoreCase))
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
            if (prefix.Equals("statuses", StringComparison.CurrentCultureIgnoreCase))
            {
                var statusInt = int.Parse(label);
                return ((Status)statusInt).ToString();
            }
            return label;
        }


        public string Id { get; }
        public string Value { get; }
        public string Label { get; }
        public bool Selected { get; set; }
    }
}
