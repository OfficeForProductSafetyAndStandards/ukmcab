using UKMCAB.Core.Security;
using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Shared
{
    public class AuditLogHistoryViewModel
    {
        public readonly string[] PublicAuditActionsToShow =
        {
            AuditCABActions.Published,
            AuditCABActions.Archived,
            AuditCABActions.UnarchivedToDraft,
            AuditCABActions.LegislativeAreaAdded,
            AuditCABActions.LegislativeAreaReviewDateAdded,
            AuditCABActions.LegislativeAreaReviewDateUpdated
        };

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
                    UserGroup = Roles.NameFor(al.UserRole),
                    DateAndTime = al.DateTime,
                    Action = NormaliseAction(al.Action),
                    InternalComment = al.Comment,
                    PublicComment = al.PublicComment,
                });

            Pagination = new PaginationViewModel
            {
                ResultsPerPage = resultsPerPage,
                Total = audits.Count,
                PageNumber = pageNumber,
                TabId = "history"
            };
        }


        public AuditLogHistoryViewModel(
            IEnumerable<Document?> documents,
            int pageNumber,
            bool showLoggedInAuditActions = false)
        {
            documents = documents
                .Where(d =>
                        d.StatusValue == Status.Published ||
                        d.StatusValue == Status.Archived ||
                        d.StatusValue == Status.Historical
                    )
                .OrderBy(d => d.LastUpdatedDate);

            List<Audit>? auditLog;
            if (showLoggedInAuditActions)
            {
                auditLog = documents
                    .SelectMany(d => d.AuditLog)
                    .ToList();
            }
            else
            {
                auditLog = documents
                    .SelectMany(d => d.AuditLog.Where(a => AuditActionIsPublicAction(a)))
                    .ToList();
            }

            if (auditLog.Any())
            {
                auditLog = auditLog
                    .GroupBy(a => new { a.Action, a.DateTime, a.UserId })
                    .Select(g => g.First())
                    .ToList();
            }

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
                    UserGroup = Roles.NameFor(al.UserRole),
                    DateAndTime = al.DateTime,
                    Action = NormaliseAction(al.Action),
                    InternalComment = al.Comment,
                    PublicComment = al.PublicComment
                });

            Pagination = new PaginationViewModel
            {
                ResultsPerPage = resultsPerPage,
                Total = auditLog.Count,
                PageNumber = pageNumber,
                TabId = "history"
            };

            bool AuditActionIsPublicAction(Audit a)
            {
                return PublicAuditActionsToShow.Any(action => action.Equals(a.Action));
            }
        }

        private static string NormaliseAction(string action)
        {
            switch (action)
            {
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
                    return "Change contact email";
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

        public IEnumerable<AuditHistoryItem> AuditHistoryItems { get; }

        public PaginationViewModel Pagination { get; set; }
    }


    public class AuditHistoryItem
    {
        public DateTime DateAndTime { get; set; }
        public string Date => DateAndTime.ToString("dd/MM/yyyy");
        public string Time => DateAndTime.ToString("HH:mm");
        public string? Username { get; init; }
        public string? UserId { get; init; }
        public string? UserGroup { get; init; }
        public string? Action { get; init; }
        public string? InternalComment { get; init; }
        public string? PublicComment { get; init; }
        public bool? IsUserInputComment { get; init; }
        public bool? IsUserEnteredPublicComment { get; init; }
    }
}