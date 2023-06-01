using Microsoft.AspNetCore.Html;
using UKMCAB.Web.UI.Models.ViewModels.Shared;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin
{
    public class CABManagementViewModel : ILayoutModel
    {
        public string? Title => "CAB management";
        public string Filter { get; set; }
        public string Sort { get; set; }
        public List<CABManagementItemViewModel> CABManagementItems { get; set; }
        public int PageNumber { get; set; } = 1;
        public PaginationViewModel Pagination { get; set; }

        public HtmlString GetAriaSort(string sortName)
        {
            if (Sort.StartsWith(sortName, StringComparison.InvariantCultureIgnoreCase))
            {
                return Sort.EndsWith("desc") ? new HtmlString("descending") : new HtmlString("ascending");
            }

            return new HtmlString("none");
        }

        public HtmlString GetSortQueryValue(string sortName)
        {
            if (Sort.StartsWith(sortName, StringComparison.InvariantCultureIgnoreCase))
            {
                return Sort.EndsWith("desc") ? new HtmlString(sortName) : new HtmlString($"{sortName}-desc");
            }

            return new HtmlString(sortName);
        }
    }
}
