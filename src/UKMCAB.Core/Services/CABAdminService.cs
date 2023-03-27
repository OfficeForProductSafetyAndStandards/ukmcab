using UKMCAB.Common;
using UKMCAB.Common.Exceptions;
using UKMCAB.Core.Models;

namespace UKMCAB.Core.Services
{
    public class CABAdminService : ICABAdminService
    {
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
            return documents.Any();
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

        public async Task<Document> CreateDocumentAsync(string userEmail, Document document)
        {
            var documentExists = await DocumentWithKeyIdentifiersExistsAsync(document);

            Rule.IsFalse(documentExists, "CAB name or number already exists in database");
            
            var createdDate = DateTime.Now;
            document.CABId = Guid.NewGuid().ToString();
            document.CreatedBy = userEmail;
            document.CreatedDate = createdDate;
            document.LastModifiedBy = userEmail;
            document.LastModifiedDate = createdDate;
            document.IsLatest = true;
            return await _cabRepostitory.CreateAsync(document);
        }

        public async Task<bool> UpdateOrCreateDraftDocumentAsync(string userEmail, Document draft)
        {
            var currentDateTime = DateTime.UtcNow;
            if (draft.IsPublished && draft.IsLatest)
            {
                var publishedVersion = await FindPublishedDocumentByCABIdAsync(draft.CABId);
                Rule.IsTrue(publishedVersion.IsLatest, $"Invalid document for creating draft, CAB Id: {draft.CABId}");
                publishedVersion.IsLatest = false;
                Rule.IsTrue(await _cabRepostitory.Update(publishedVersion),
                    $"Failed to update published version during draft update, CAB Id: {draft.CABId}");

                draft.IsPublished = false;
                draft.id = string.Empty;
                draft.CreatedBy = userEmail;
                draft.CreatedDate = currentDateTime;
                draft.LastModifiedBy = userEmail;
                draft.LastModifiedDate = currentDateTime;
                draft.IsLatest = true;
                var newdraft = await _cabRepostitory.CreateAsync(draft);
                Rule.IsFalse(newdraft == null,
                    $"Failed to create draft version during draft update, CAB Id: {draft.CABId}");

                return true;
            }

            if (draft.IsLatest)
            {
                draft.LastModifiedBy = userEmail;
                draft.LastModifiedDate = currentDateTime;
                Rule.IsTrue(await _cabRepostitory.Update(draft), $"Failed to update draft , CAB Id: {draft.CABId}");

                return true;
            }

            throw new DomainException($"Invalid document for creating draft, CAB Id: {draft.CABId}");
        }

        public async Task<bool> DeleteDraftDocumentAsync(string cabId)
        {
            var documents = await FindAllDocumentsByCABIdAsync(cabId);
            Rule.IsTrue(documents.Any(d => d.IsLatest) && documents.Count(d => d.IsLatest) == 1 , $"Error finding the latest document to delete, CAB Id: {cabId}");
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
                Rule.IsTrue(await _cabRepostitory.Update(publishedVersion),
                    $"Failed to update published version during draft update, CAB Id: {cabId}");
            }
            Rule.IsTrue(await _cabRepostitory.Delete(latest), $"Failed to delete draft version, CAB Id: {cabId}");
            return true;
        }
    }
}
