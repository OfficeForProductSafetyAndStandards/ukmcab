namespace UKMCAB.Core.Services.CAB
{
    using System;
    using System.Threading.Tasks;
    using UKMCAB.Data.Models.Users;
    using UKMCAB.Data.Models;

    public interface IUserNoteService
    {
        Task<List<UserNote>> GetAllUserNotesForCabDocumentId(Guid cabDocumentId);

        Task<UserNote> GetUserNote(Guid cabDocumentId, Guid userNoteId);

        Task CreateUserNote(UserAccount userAccount, Guid cabDocumentId, string note);

        Task DeleteUserNote(Guid cabDocumentId, Guid userNoteId);
    }
}
