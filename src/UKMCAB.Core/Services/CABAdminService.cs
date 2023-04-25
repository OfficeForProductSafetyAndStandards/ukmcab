using UKMCAB.Common;
using UKMCAB.Data.CosmosDb.Services;
using UKMCAB.Data.Models;
using UKMCAB.Data;
using UKMCAB.Data.Search.Services;

namespace UKMCAB.Core.Services
{
    public class CABAdminService : ICABAdminService
    {
        /*
          NOTE FROM KRIS, RE: EDITING/UPDATING CABS
            We now have a ICachedSearchService.  When a *published* CAB changes, ideally we should manually update the search index for the associated item and then call `ICachedSearchService.ClearAsync(CABId)`
            As this will clear all search results from the cache that contained _THAT_ cab id.

            ALSO: when updating a published cab, call `ICachedPublishedCabService.ClearAsync`, as ICachedPublishedCabService refs ICABAdminService, 
            you might want to do updating via ICachedPublishedCabService and then clear the cache that way (otherwise if you ref ICachedPublishedCabService from here, you'll get circular dependency)
         */

        private readonly ICABRepository _cabRepostitory;
        private readonly ICachedSearchService _cachedSearchService;
        private readonly ICachedPublishedCabService _cachedPublishedCabService;

        public CABAdminService(ICABRepository cabRepostitory, ICachedSearchService cachedSearchService, ICachedPublishedCabService cachedPublishedCabService)
        {
            _cabRepostitory = cabRepostitory;
            _cachedSearchService = cachedSearchService;
            _cachedPublishedCabService = cachedPublishedCabService;
        }

        public async Task<bool> DocumentWithKeyIdentifiersExistsAsync(Document document)
        {
            var documents = await _cabRepostitory.Query<Document>(d =>
                d.Name.Equals(document.Name, StringComparison.InvariantCultureIgnoreCase) ||
                d.CABNumber.Equals(document.CABNumber) ||
                (!string.IsNullOrWhiteSpace(document.UKASReference) && d.UKASReference.Equals(document.UKASReference))
            );
            return documents.Any(d => !d.CABId.Equals(document.CABId));
        }

        public async Task<Document> FindPublishedDocumentByCABIdAsync(string id)
        {
            var doc = await _cabRepostitory.Query<Document>(d => d.StatusValue == Status.Published && d.CABId.Equals(id));
            return doc.Any() && doc.Count == 1 ? doc.First() : null;
        }

        public async Task<List<Document>> FindAllDocumentsByCABIdAsync(string id)
        {
            var docs = await _cabRepostitory.Query<Document>(d =>
                d.CABId.Equals(id, StringComparison.CurrentCultureIgnoreCase));
            return docs;
        }

        public async Task<List<Document>> FindAllWorkQueueDocuments()
        {
            // TODO: Archived docs need to be included once this functionality is developed 
            var docs = await _cabRepostitory.Query<Document>(d =>
                d.StatusValue == Status.Draft || d.StatusValue == Status.Archived);
            return docs;
        }

        public async Task<Document> GetLatestDocumentAsync(string cabId)
        {
            var documents = await FindAllDocumentsByCABIdAsync(cabId);
            // if a newly create cab or a draft version exists this will be the latest version, there should be no more than one
            if (documents.Any(d => d.StatusValue == Status.Created || d.StatusValue == Status.Draft))
            {
                return documents.Single(d => d.StatusValue == Status.Created || d.StatusValue == Status.Draft);
            }
            // if no draft or created version exists then see if a published version exists, again should only ever be one
            if (documents.Any(d => d.StatusValue == Status.Published))
            {
                return documents.Single(d => d.StatusValue == Status.Published);
            }

            return null;
        }

        public IAsyncEnumerable<string> GetAllCabIds()
        {
            return _cabRepostitory.GetItemLinqQueryable().Select(x => x.CABId).AsAsyncEnumerable();
        }

        public async Task<Document> CreateDocumentAsync(string userEmail, Document document)
        {
            var documentExists = await DocumentWithKeyIdentifiersExistsAsync(document);

            Guard.IsFalse(documentExists, "CAB name or number already exists in database");
            
            var createdDate = DateTime.Now;
            document.CABId = Guid.NewGuid().ToString();
            document.CreatedBy = userEmail;
            document.CreatedDate = createdDate;
            document.LastModifiedBy = userEmail;
            document.LastModifiedDate = createdDate;
            document.StatusValue = Status.Created;
            return await _cabRepostitory.CreateAsync(document);
        }

        public async Task<Document> UpdateOrCreateDraftDocumentAsync(string userEmail, Document draft, bool saveAsDraft = false)
        {
            var currentDateTime = DateTime.UtcNow;
            if (draft.StatusValue == Status.Published)
            {
                draft.StatusValue = Status.Draft;
                draft.id = string.Empty;
                draft.CreatedBy = userEmail;
                draft.CreatedDate = currentDateTime;
                draft.LastModifiedBy = userEmail;
                draft.LastModifiedDate = currentDateTime;
                var newdraft = await _cabRepostitory.CreateAsync(draft);
                Guard.IsFalse(newdraft == null,
                    $"Failed to create draft version during draft update, CAB Id: {draft.CABId}");
                return newdraft;
            }

            if (draft.StatusValue == Status.Created && saveAsDraft)
            {
                draft.StatusValue = Status.Draft;
            }
            if (draft.StatusValue == Status.Created || draft.StatusValue == Status.Draft)
            {
                draft.LastModifiedBy = userEmail;
                draft.LastModifiedDate = currentDateTime;
                Guard.IsTrue(await _cabRepostitory.Update(draft), $"Failed to update draft , CAB Id: {draft.CABId}");
                return draft;
            }

            throw new Exception($"Invalid document for creating or updating draft, CAB Id: {draft.CABId}");
        }

        public async Task<bool> DeleteDraftDocumentAsync(string cabId)
        {
            var documents = await FindAllDocumentsByCABIdAsync(cabId);
            // currently we only delete newly created docs that haven't been saved as draft
            Guard.IsTrue(documents.Count == 1 && documents.First().StatusValue == Status.Created, $"Error finding the document to delete, CAB Id: {cabId}");
            Guard.IsTrue(await _cabRepostitory.Delete(documents.First()), $"Failed to delete draft version, CAB Id: {cabId}");
            return true;
        }

        public async Task<Document> PublishDocumentAsync(string userEmail, Document latestDocument)
        {
            Guard.IsTrue(latestDocument.StatusValue == Status.Created || latestDocument.StatusValue == Status.Draft, $"Submitted document for publishing incorrectly flagged, CAB Id: {latestDocument.CABId}");
            var currentDateTime = DateTime.UtcNow;
            var publishedVersion = await FindPublishedDocumentByCABIdAsync(latestDocument.CABId);
            if (publishedVersion != null)
            {
                publishedVersion.StatusValue = Status.Historical;
                Guard.IsTrue(await _cabRepostitory.Update(publishedVersion),
                    $"Failed to update published version during draft publish, CAB Id: {latestDocument.CABId}");
            }
            latestDocument.PublishedBy = userEmail;
            latestDocument.PublishedDate = currentDateTime;
            latestDocument.StatusValue = Status.Published;
            latestDocument.RandomSort = Guid.NewGuid().ToString();
            Guard.IsTrue(await _cabRepostitory.Update(latestDocument),
                $"Failed to publish latest version during draft publish, CAB Id: {latestDocument.CABId}");

            // TODO: look at introducing CAB targeted index updates rather than complete index update
            await _cachedSearchService.ReIndexAsync();
            await _cachedPublishedCabService.ClearAsync(latestDocument.CABId);

            return latestDocument;
        }
    }
}
