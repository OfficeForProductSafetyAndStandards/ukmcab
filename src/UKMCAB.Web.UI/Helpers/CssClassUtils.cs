using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Helpers;

public class CssClassUtils
{
    public static string CabStatusStyle(Status status)
    {
        return status switch
        {
            Status.Published => "cab-status-tag--published",
            Status.Draft => "cab-status-tag--draft",
            Status.Archived => "cab-status-tag--archived",
            _ => ""
        };
    }
}