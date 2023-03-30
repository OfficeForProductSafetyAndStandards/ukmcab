using UKMCAB.Common;
using UKMCAB.Core.Models;

namespace UKMCAB.Core.Services
{
    public class CABAdminService : ICABAdminService
    {
        // Guide to document status types
        // IsPublished: true, IsLatest: true: The latest published version, there is no draft version in progress
        // IsPublished: true, IsLatest: false: The latest published version, there is a newer draft version in progress
        // IsPublished: false, IsLatest: true: The latest draft version. There may be a prior published version, PublishedDate should indicate this
        // IsPublished: false, IsLatest: false: Historical version that has been superceded by a more recent published version or versions

        private readonly ICABRepository _cabRepostitory;

        public CABAdminService(ICABRepository cabRepostitory)
        {
            _cabRepostitory = cabRepostitory;
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
            var doc = await _cabRepostitory.Query<Document>(d => d.IsPublished && d.CABId.Equals(id));
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
                d.IsLatest && !d.IsPublished);
            return docs;
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
            document.IsLatest = true;
            return await _cabRepostitory.CreateAsync(document);
        }

        public async Task<Document> UpdateOrCreateDraftDocumentAsync(string userEmail, Document draft)
        {
            var currentDateTime = DateTime.UtcNow;
            if (draft.IsPublished && draft.IsLatest)
            {
                var publishedVersion = await FindPublishedDocumentByCABIdAsync(draft.CABId);
                Guard.IsTrue(publishedVersion.IsLatest, $"Invalid document for creating draft, CAB Id: {draft.CABId}");
                publishedVersion.IsLatest = false;
                Guard.IsTrue(await _cabRepostitory.Update(publishedVersion),
                    $"Failed to update published version during draft update, CAB Id: {draft.CABId}");

                draft.IsPublished = false;
                draft.id = string.Empty;
                draft.CreatedBy = userEmail;
                draft.CreatedDate = currentDateTime;
                draft.LastModifiedBy = userEmail;
                draft.LastModifiedDate = currentDateTime;
                draft.IsLatest = true;
                var newdraft = await _cabRepostitory.CreateAsync(draft);
                Guard.IsFalse(newdraft == null,
                    $"Failed to create draft version during draft update, CAB Id: {draft.CABId}");

                return newdraft;
            }

            if (draft.IsLatest)
            {
                draft.LastModifiedBy = userEmail;
                draft.LastModifiedDate = currentDateTime;
                Guard.IsTrue(await _cabRepostitory.Update(draft), $"Failed to update draft , CAB Id: {draft.CABId}");

                return draft;
            }

            throw new Exception($"Invalid document for creating draft, CAB Id: {draft.CABId}");
        }

        public async Task<bool> DeleteDraftDocumentAsync(string cabId)
        {
            var documents = await FindAllDocumentsByCABIdAsync(cabId);
            Guard.IsTrue(documents.Any(d => d.IsLatest) && documents.Count(d => d.IsLatest) == 1 , $"Error finding the latest document to delete, CAB Id: {cabId}");
            var latest = documents.Single(d => d.IsLatest);
            if (latest.IsPublished)
            {
                // No draft version to delete
                return true;
            }
            if (documents.Any(d => d.IsPublished))
            {
                // Need to make the published version the latest again
                var publishedVersion = documents.Single(d => d.IsPublished);
                publishedVersion.IsLatest = true;
                Guard.IsTrue(await _cabRepostitory.Update(publishedVersion),
                    $"Failed to update published version during draft update, CAB Id: {cabId}");
            }
            Guard.IsTrue(await _cabRepostitory.Delete(latest), $"Failed to delete draft version, CAB Id: {cabId}");
            return true;
        }

        public async Task<Document> PublishDocumentAsync(string userEmail, Document latestDocument)
        {
            Guard.IsTrue(latestDocument.IsLatest && !latestDocument.IsPublished, $"Submitted document for publishing incorrectly flagged, CAB Id: {latestDocument.CABId}");
            var currentDateTime = DateTime.UtcNow;
            var publishedVersion = await FindPublishedDocumentByCABIdAsync(latestDocument.CABId);
            if (publishedVersion != null)
            {
                publishedVersion.IsLatest = false;
                publishedVersion.IsPublished = false;
                Guard.IsTrue(await _cabRepostitory.Update(publishedVersion),
                    $"Failed to update published version during draft publish, CAB Id: {latestDocument.CABId}");
            }
            latestDocument.PublishedBy = userEmail;
            latestDocument.PublishedDate = currentDateTime;
            latestDocument.IsPublished = true;
            latestDocument.RandomSort = Guid.NewGuid().ToString();
            Guard.IsTrue(await _cabRepostitory.Update(latestDocument),
                $"Failed to publish latest version during draft publish, CAB Id: {latestDocument.CABId}");
            return latestDocument;
        }
    }
}
