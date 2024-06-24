namespace UKMCAB.Web.UI.Models.ViewModels.Shared
{
    using UKMCAB.Core.Security;
    using UKMCAB.Data.Models;

    public class UserNoteListViewModel
    {
        public const int resultsPerPage = 10;

        public UserNoteListViewModel(Guid cabDocumentId, List<UserNote> userNotes, int pageNumber, bool cabHasDraft)
        {
            CabDocumentId = cabDocumentId;

            UserNoteItems = userNotes
                .OrderByDescending(u => u.DateTime)
                .Skip((pageNumber - 1) * resultsPerPage)
                .Take(resultsPerPage)
                .Select(u => new UserNoteListItemViewModel
                {
                    Id = u.Id,
                    CabDocumentId = cabDocumentId,
                    UserId = u.UserId,
                    UserName = u.UserName,
                    UserGroup = Roles.NameFor(u.UserRole),
                    DateAndTime = u.DateTime,
                    Note = u.Note,
                });

            Pagination = new PaginationViewModel
            {
                ResultsPerPage = resultsPerPage,
                Total = userNotes.Count,
                PageNumber = pageNumber,
                TabId = "usernotes",
            };

            CabHasDraft = cabHasDraft;
        }

        public UserNoteListViewModel(Guid cabDocumentId, List<UserNote> userNotes, int pageNumber)
            : this( cabDocumentId , userNotes, pageNumber, false)
        {
        }

        public Guid CabDocumentId { get; set; }

        public IEnumerable<UserNoteListItemViewModel> UserNoteItems { get; }

        public PaginationViewModel Pagination { get; set; }

        public bool CabHasDraft { get; set; }
    }
}