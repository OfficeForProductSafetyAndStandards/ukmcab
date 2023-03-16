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
            Label = SanitiseLabel(filterValue);
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
            if (string.IsNullOrWhiteSpace(label))
            {
                return string.Empty;
            }
            label = label.Replace("-", " ");
            var labelParts = label.Split(' ');
            labelParts[0] = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(labelParts[0]);
            return string.Join(" ", labelParts);
        }


        public string Id { get; }
        public string Value { get; }
        public string Label { get; }
        public bool Selected { get; set; }
    }
}
