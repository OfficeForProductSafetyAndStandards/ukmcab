using UKMCAB.Common;
using UKMCAB.Data.CosmosDb.Services;
using UKMCAB.Data.Models;
using UKMCAB.Data;
using UKMCAB.Data.Search.Services;
using UKMCAB.Identity.Stores.CosmosDB;

namespace UKMCAB.Core.Services
{
    public class CABAdminService : ICABAdminService
    {
        private readonly ICABRepository _cabRepostitory;
        private readonly ICachedSearchService _cachedSearchService;
        private readonly ICachedPublishedCABService _cachedPublishedCabService;

        public CABAdminService(ICABRepository cabRepostitory, ICachedSearchService cachedSearchService, ICachedPublishedCABService cachedPublishedCabService)
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

        public async Task<Document> CreateDocumentAsync(UKMCABUser user, Document document, bool saveAsDraft = false)
        {
            var documentExists = await DocumentWithKeyIdentifiersExistsAsync(document);

            Guard.IsFalse(documentExists, "CAB name or number already exists in database");
            
            var createdDate = DateTime.Now;
            var auditItem = new Audit(user, createdDate);
            document.CABId = Guid.NewGuid().ToString().Md5();
            document.Created = auditItem;
            document.LastUpdated = auditItem;
            document.StatusValue = saveAsDraft ? Status.Draft : Status.Created;
            return await _cabRepostitory.CreateAsync(document);
        }

        public async Task<Document> UpdateOrCreateDraftDocumentAsync(UKMCABUser user, Document draft, bool saveAsDraft = false)
        {
            var audit = new Audit(user, DateTime.UtcNow);
            if (draft.StatusValue == Status.Published)
            {
                draft.StatusValue = saveAsDraft ? Status.Draft : Status.Created;
                draft.id = string.Empty;
                draft.Created = audit;
                draft.LastUpdated = audit;
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
                draft.LastUpdated = audit;
                Guard.IsTrue(await _cabRepostitory.Update(draft), $"Failed to update draft , CAB Id: {draft.CABId}");
                return draft;
            }

            throw new Exception($"Invalid document for creating or updating draft, CAB Id: {draft.CABId}");
        }

        public async Task<bool> DeleteDraftDocumentAsync(string cabId)
        {
            var documents = await FindAllDocumentsByCABIdAsync(cabId);
            // currently we only delete newly created docs that haven't been saved as draft
            var createdDoc = documents.SingleOrDefault(d => d.StatusValue == Status.Created);
            Guard.IsTrue(createdDoc != null, $"Error finding the document to delete, CAB Id: {cabId}");
            Guard.IsTrue(await _cabRepostitory.Delete(createdDoc), $"Failed to delete draft version, CAB Id: {cabId}");
            return true;
        }

        public async Task<Document> PublishDocumentAsync(UKMCABUser user, Document latestDocument)
        {
            Guard.IsTrue(latestDocument.StatusValue == Status.Created || latestDocument.StatusValue == Status.Draft, $"Submitted document for publishing incorrectly flagged, CAB Id: {latestDocument.CABId}");
            var publishedVersion = await FindPublishedDocumentByCABIdAsync(latestDocument.CABId);
            var audit = new Audit(user, DateTime.UtcNow);
            if (publishedVersion == null)
            {
                // If there is no published version then we set the published date
                latestDocument.Published = audit;
            }
            else
            {
                publishedVersion.StatusValue = Status.Historical;
                Guard.IsTrue(await _cabRepostitory.Update(publishedVersion),
                    $"Failed to update published version during draft publish, CAB Id: {latestDocument.CABId}");
                await _cachedSearchService.RemoveFromIndexAsync(publishedVersion.id);
            }

            latestDocument.LastUpdated = audit;
            latestDocument.StatusValue = Status.Published;
            latestDocument.RandomSort = Guid.NewGuid().ToString();
            Guard.IsTrue(await _cabRepostitory.Update(latestDocument),
                $"Failed to publish latest version during draft publish, CAB Id: {latestDocument.CABId}");

            await RefreshCaches(latestDocument.CABId);

            return latestDocument;
        }

        public async Task<Document> ArchiveDocumentAsync(UKMCABUser user, Document latestDocument, string archiveReason)
        {
            var publishedVersion = await FindPublishedDocumentByCABIdAsync(latestDocument.CABId);
            Guard.IsTrue(publishedVersion != null, $"Submitted document for archiving incorrectly flagged, CAB Id: {latestDocument.CABId}");
            publishedVersion.StatusValue = Status.Archived;
            publishedVersion.Archived = new Audit(user, DateTime.UtcNow);
            publishedVersion.ArchivedReason = archiveReason;
            Guard.IsTrue(await _cabRepostitory.Update(publishedVersion),
                $"Failed to archive published version, CAB Id: {latestDocument.CABId}");

            await _cachedSearchService.RemoveFromIndexAsync(publishedVersion.id);
            await RefreshCaches(latestDocument.CABId);
            return publishedVersion;
        }

        private async Task RefreshCaches(string cabId)
        {
            await _cachedSearchService.ReIndexAsync();
            await _cachedSearchService.ClearAsync();
            await _cachedSearchService.ClearAsync(cabId);
            await _cachedPublishedCabService.ClearAsync(cabId);
        }
    }


}
