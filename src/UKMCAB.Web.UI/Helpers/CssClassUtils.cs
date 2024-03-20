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
    public static string LAStatusStyle(LAStatus status)
    {
        return status switch
        {
            LAStatus.Published => "govuk-tag--green",
            LAStatus.Approved => "govuk-tag--green",
            LAStatus.Declined => "govuk-tag--orange",
            LAStatus.Draft => "govuk-tag--blue",   
            _ => "govuk-tag--yellow"
        };
    }
}