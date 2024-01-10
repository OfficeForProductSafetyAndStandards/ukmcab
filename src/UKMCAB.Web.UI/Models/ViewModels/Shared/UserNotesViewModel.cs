namespace UKMCAB.Web.UI.Models.ViewModels.Shared
{
    using UKMCAB.Core.Security;
    using UKMCAB.Data.Models;

    public class UserNotesViewModel
    {
        public const int resultsPerPage = 10;

        public UserNotesViewModel(List<UserNote> userNotes, int pageNumber)
        {
            UserNoteItems = userNotes
                .OrderByDescending(u => u.DateTime)
                .Skip((pageNumber - 1) * resultsPerPage)
                .Take(resultsPerPage)
                .Select(u => new UserNoteItemViewModel
                {
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
        }

        public IEnumerable<UserNoteItemViewModel> UserNoteItems { get; }

        public PaginationViewModel Pagination { get; set; }
    }
}