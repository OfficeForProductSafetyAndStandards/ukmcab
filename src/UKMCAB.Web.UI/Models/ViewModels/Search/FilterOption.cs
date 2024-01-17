using System.Globalization;
using UKMCAB.Common.Extensions;
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

        private static string SanitiseLabel(string prefix, string value)
        {
            if (prefix.Equals("bodytypes", StringComparison.CurrentCultureIgnoreCase))
            {
                switch (value)
                {
                    case "third-country-body":
                        return "Third country body";
                    case "Overseas Body":
                        return "Overseas body";
                    default:
                        return value;
                }
            }
            if (prefix.Equals("statuses", StringComparison.CurrentCultureIgnoreCase))
            {
                var statusInt = int.Parse(value);
                return ((Status)statusInt).ToString();
            }
            if (prefix.Equals("substatuses", StringComparison.CurrentCultureIgnoreCase))
            {
                var subStatus = Enum.Parse<SubStatus>(value);
          
                switch (subStatus)
                {
                    case SubStatus.PendingApprovalToPublish:
                        return "To publish CAB";
                    case SubStatus.PendingApprovalToArchive:
                        return "To archive CAB";
                    case SubStatus.PendingApprovalToUnarchivePublish:
                        return "To unarchive and publish CAB";
                    case SubStatus.PendingApprovalToUnarchive:
                        return "To unarchive CAB as draft";
                    default:
                        return subStatus.GetEnumDescription();;
                }
            }
            if (prefix.Equals("usergroups", StringComparison.CurrentCultureIgnoreCase))
            {
                return value.ToUpper();
            }
            return value;
        }


        public string Id { get; }
        public string Value { get; }
        public string Label { get; }
        public bool Selected { get; set; }
    }
}
