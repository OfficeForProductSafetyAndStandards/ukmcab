using UKMCAB.Core.Security;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Web.UI.Models.ViewModels.Shared
{
    public class AuditLogHistoryViewModel
    {
        public readonly string[] PublicAuditActionsToShow = { AuditCABActions.Published, AuditCABActions.Archived, AuditCABActions.UnarchiveRequest };
        public readonly string[] OPSSUserAuditActionsToShow = { AuditCABActions.Published, AuditCABActions.Archived, AuditCABActions.UnarchiveRequest, AuditCABActions.Saved, AuditCABActions.Unarchived, AuditCABActions.Created };


        public const int resultsPerPage = 10;

        public AuditLogHistoryViewModel(List<Audit> audits, int pageNumber)
        {
            if (audits == null)
            {
                audits = new List<Audit>();
            }
            AuditHistoryItems = audits
                .OrderByDescending(al => al.DateTime)
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
                });

            Pagination = new PaginationViewModel
            {
                ResultsPerPage = resultsPerPage,
                Total = audits.Count,
                PageNumber = pageNumber,
                TabId = "history"
            };
        }


        public AuditLogHistoryViewModel(IEnumerable<Document> documents, UserAccount userAccount, int pageNumber)
        {
            IsOPSSUser = userAccount != null && userAccount.Role == Roles.OPSS.Id;
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
                .OrderByDescending(al => al.DateTime)
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
                // CAB audit mappings
                case AuditCABActions.UnarchiveRequest:
                    return "Unarchive request";
                // User audit mappings
                case AuditUserActions.UserAccountRequest:
                    return "User access request";
                case AuditUserActions.ApproveAccountRequest:
                    return "User access approved";
                case AuditUserActions.DeclineAccountRequest:
                    return "User access declined";
                case AuditUserActions.LockAccountRequest:
                    return "User account locked";
                case AuditUserActions.UnlockAccountRequest:
                    return "User account unlocked";
                case AuditUserActions.ArchiveAccountRequest:
                    return "User account archived";
                case AuditUserActions.UnarchiveAccountRequest:
                    return "User account unarchived";
                case AuditUserActions.ChangeOfContactEmailAddress:
                    return "Change email";
                case AuditUserActions.ChangeOfOrganisation:
                    return "Change organisation";
                case AuditUserActions.ChangeOfRole:
                    return "Change user group";
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
