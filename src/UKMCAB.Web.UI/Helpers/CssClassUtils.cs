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
            LAStatus.ApprovedByOpssAdmin or 
                LAStatus.ApprovedToRemoveByOpssAdmin or 
                LAStatus.ApprovedToArchiveAndArchiveScheduleByOpssAdmin or 
                LAStatus.ApprovedToUnarchiveByOPSS or 
                LAStatus.ApprovedToArchiveAndRemoveScheduleByOpssAdmin or 
                LAStatus.Published 
                => "govuk-tag--green",
            LAStatus.Approved or 
                LAStatus.ApprovedToUnarchiveByOPSS 
                => "govuk-tag--turquoise",
            LAStatus.Declined or 
                LAStatus.DeclinedByOpssAdmin or 
                LAStatus.DeclinedToRemoveByOGD or 
                LAStatus.DeclinedToRemoveByOPSS or 
                LAStatus.DeclinedToUnarchiveByOGD or 
                LAStatus.DeclinedToUnarchiveByOPSS or 
                LAStatus.DeclinedToArchiveAndArchiveScheduleByOGD or 
                LAStatus.DeclinedToArchiveAndArchiveScheduleByOPSS or 
                LAStatus.DeclinedToArchiveAndRemoveScheduleByOGD or 
                LAStatus.DeclinedToArchiveAndRemoveScheduleByOPSS 
                => "govuk-tag--orange",
            LAStatus.Draft => "govuk-tag--blue",   
            _ => "govuk-tag--yellow"
        };
    }
}