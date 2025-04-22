namespace UKMCAB.Core.Services.CAB
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using UKMCAB.Common;
    using UKMCAB.Data.Interfaces.Services.CAB;
    using UKMCAB.Data.Interfaces.Services.CachedCAB;
    using UKMCAB.Data.Models;
    using UKMCAB.Data.Models.Users;

    public class UserNoteService : IUserNoteService
    {
        private readonly ICABRepository _cabRepository;
        private readonly ICachedPublishedCABService _cachedPublishedCabService;

        public UserNoteService(ICABRepository cabRepository, ICachedPublishedCABService cachedPublishedCabService)
        {
            _cabRepository = cabRepository;
            _cachedPublishedCabService = cachedPublishedCabService;
        }

        public async Task<List<UserNote>> GetAllUserNotesForCabDocumentId(Guid cabDocumentId)
        {
            Document cab = await GetCabDocumentByCabDocumentId(cabDocumentId);

            return cab.GovernmentUserNotes;
        }

        public async Task<UserNote> GetUserNote(Guid cabDocumentId, Guid userNoteId)
        {
            Document cab = await GetCabDocumentByCabDocumentId(cabDocumentId);
           
            UserNote userNote = cab.GovernmentUserNotes.Where(u => u.Id == userNoteId).FirstOrDefault();

            Guard.IsNotNull(userNote, () => new Exception(
               $"UserNote not found. UserNote ID: {userNoteId}."));

            return userNote;
        }

        public async Task CreateUserNote(UserAccount userAccount, Guid cabDocumentId, string note)
        {
            Document cab = await GetCabDocumentByCabDocumentId(cabDocumentId);

            var userNote = new UserNote
            {
                Id = Guid.NewGuid(),
                DateTime = DateTime.Now,
                UserId = userAccount.Id,
                UserRole = userAccount.Role,
                UserName = $"{userAccount.FirstName} {userAccount.Surname}",
                Note = note
            };

            cab.GovernmentUserNotes.Add(userNote);
            await _cabRepository.UpdateAsync(cab);
            await _cachedPublishedCabService.ClearAsync(cab.CABId, cab.URLSlug);
        }

        public async Task DeleteUserNote(Guid cabDocumentId, Guid userNoteId)
        {
            Document cab = await GetCabDocumentByCabDocumentId(cabDocumentId);

            UserNote userNote = cab.GovernmentUserNotes.Where(u => u.Id == userNoteId).FirstOrDefault();

            if (userNote != null)
            {
                cab.GovernmentUserNotes.Remove(userNote);
                await _cabRepository.UpdateAsync(cab);
                await _cachedPublishedCabService.ClearAsync(cab.CABId, cab.URLSlug);
            }
        }

        private async Task<Document> GetCabDocumentByCabDocumentId(Guid cabDocumentId)
        {
            List<Document> results = await _cabRepository.Query<Document>(d =>
                d.id.Equals(cabDocumentId.ToString(), StringComparison.CurrentCultureIgnoreCase));

            var cab = results.FirstOrDefault();

            Guard.IsNotNull(cab, () => new Exception(
               $"CAB document not found. Document ID {cabDocumentId}. Note: this parameter is the Document.id, not the Document.CABId."));

            return cab;
        }
    }
}
