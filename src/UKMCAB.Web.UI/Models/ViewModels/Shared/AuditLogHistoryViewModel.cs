using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Shared
{
    public class AuditLogHistoryViewModel
    {
        public readonly string[] AuditActionsToShow = { AuditActions.Published, AuditActions.Archived, AuditActions.UnarchiveRequest };
        public const int resultsPerPage = 10;
        public AuditLogHistoryViewModel(IEnumerable<Document> documents, int pageNumber)
        {
            documents = documents
                .Where(d => d.StatusValue == Status.Published || d.StatusValue == Status.Archived || d.StatusValue == Status.Historical)
                .OrderBy(d => d.LastUpdatedDate);

            var auditLog = documents.SelectMany(d => d.AuditLog.Where(a => AuditActionsToShow.Any(action => action.Equals(a.Action)))).ToList();

            if ((pageNumber - 1) * resultsPerPage > auditLog.Count)
            {
                pageNumber = 1;
            }
            
            AuditHistoryItems = auditLog
                .Skip((pageNumber - 1) * resultsPerPage)
                .Take(resultsPerPage)
                .Select(al => new AuditHistoryItem
                {
                    Username = al.UserName, 
                    DateAndTime = al.DateTime, 
                    Action = NormaliseAction(al.Action),
                    Comment = al.Comment
                });

            Pagination = new PaginationViewModel
            {
                ResultsPerPage = resultsPerPage,
                Total = auditLog.Count,
                PageNumber = pageNumber,
                TabId = "history"
            };
        }



        private static string NormaliseAction(string action)
        {
            switch (action)
            {
                case AuditActions.UnarchiveRequest:
                    return "Unarchive request";
                default:
                    return action;
            }
        }

        public IEnumerable<AuditHistoryItem> AuditHistoryItems { get;}

        public PaginationViewModel Pagination { get; set; }
    }


    public class AuditHistoryItem
    {
        public DateTime DateAndTime { get; set; }
        public string Date => DateAndTime.ToString("dd/MM/yyyy");
        public string Time => DateAndTime.ToString("hh:mm");
        public string Username { get; set; }
        public string Action { get; set; }
        public string Comment { get; set; }
    }
}
