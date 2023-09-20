using UKMCAB.Core.Security;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Web.UI.Models.ViewModels.Shared
{
    public class AuditLogHistoryViewModel
    {
        public readonly string[] PublicAuditActionsToShow = { AuditActions.Published, AuditActions.Archived, AuditActions.UnarchiveRequest };
        public readonly string[] OPSSUserAuditActionsToShow = { AuditActions.Published, AuditActions.Archived, AuditActions.UnarchiveRequest, AuditActions.Saved, AuditActions.Unarchived, AuditActions.Created };


        public const int resultsPerPage = 10;
        public AuditLogHistoryViewModel(IEnumerable<Document> documents, UserAccount userAccount, int pageNumber)
        {
            IsOPSSUser = userAccount.Role == Roles.OPSS.Id;
            OPSSUserId = IsOPSSUser ? userAccount.Id : string.Empty;

            documents = documents
                .Where(d => d.StatusValue == Status.Published || d.StatusValue == Status.Archived || d.StatusValue == Status.Historical)
                .OrderBy(d => d.LastUpdatedDate);

            var auditActionsToShow = IsOPSSUser ? OPSSUserAuditActionsToShow : PublicAuditActionsToShow;

            var auditLog = documents.SelectMany(d => d.AuditLog.Where(a => auditActionsToShow.Any(action => action.Equals(a.Action)))).ToList();

            if ((pageNumber - 1) * resultsPerPage > auditLog.Count)
            {
                pageNumber = 1;
            }
            
            AuditHistoryItems = auditLog
                .Skip((pageNumber - 1) * resultsPerPage)
                .Take(resultsPerPage)
                .Select(al => new AuditHistoryItem
                {
                    UserId = al.UserId,
                    Username = al.UserName, 
                    Usergroup = al.UserRole,
                    DateAndTime = al.DateTime, 
                    Action = NormaliseAction(al.Action),
                    Comment = al.Comment
                }).OrderBy(al => al.DateAndTime);

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

        public bool IsOPSSUser { get; set; }
        public string OPSSUserId { get; set; }

        public IEnumerable<AuditHistoryItem> AuditHistoryItems { get;}

        public PaginationViewModel Pagination { get; set; }
    }


    public class AuditHistoryItem
    {
        public DateTime DateAndTime { get; set; }
        public string Date => DateAndTime.ToString("dd/MM/yyyy");
        public string Time => DateAndTime.ToString("HH:mm");
        public string Username { get; set; }
        public string UserId { get; set; }
        public string Usergroup { get; set; }
        public string Action { get; set; }
        public string Comment { get; set; }
    }
}
