using UKMCAB.Common;
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

        public async Task<CABDocument> FindCABDocumentByIdAsync(string id)
        {
            var doc = await _cabRepostitory.GetByIdAsync<CABDocument>(id, id);
            return doc;
        }

        public async Task<Document> CreateCABDocumentAsync(string email, CABData cabData)
        {
            var documents = await FindCABDocumentsByNameAsync(cabData.Name);

            Rule.IsFalse(documents.Any(), "CAB name already exists in database");
            
            var createdDate = DateTime.Now;
            cabData.CABId = Guid.NewGuid().ToString();
            var document = new Document
            {
                CreatedBy = email,
                CreatedDate = createdDate,
                AuditHistory = new List<Audit>
                {
                    new()
                    {
                        User = email,
                        Description = "Created",
                        Date = createdDate
                    }
                },
                LastModifiedBy = email,
                LastModifiedDate = createdDate,
                CABData = cabData,
                State = State.Created
            };
            return await _cabRepostitory.CreateAsync(document);
        }

        public async Task<bool> UpdateCABAsync(string email, Document document)
        {
            var cab = await FindCABDocumentsByIdAsync(document.CABData.CABId);
            Rule.IsFalse(cab == null || !cab.Any(), "CAB does not exist in database");

            var updateDate = DateTime.Now;
            document.LastModifiedBy = email;
            document.LastModifiedDate = updateDate;
            var result = await _cabRepostitory.Updated(document);
            return result;
        }

        public async Task<List<Document>> FindCABDocumentsByNameAsync(string cabName)
        {
            var docs = await _cabRepostitory.Query<Document>(d =>
                d.CABData.Name.Equals(cabName, StringComparison.CurrentCultureIgnoreCase));
            return docs;
        }

        public async Task<List<Document>> FindCABDocumentsByIdAsync(string id)
        {
            var docs = await _cabRepostitory.Query<Document>(d =>
                d.CABData.CABId.Equals(id, StringComparison.CurrentCultureIgnoreCase));
            return docs;
        }


        public async Task<List<Document>> FindCABDocumentsByUKASReferenceAsync(string ukasReference)
        {
            var docs = await _cabRepostitory.Query<Document>(d =>
                d.CABData.UKASReference.Equals(ukasReference, StringComparison.CurrentCultureIgnoreCase));
            return docs;
        }

        public async Task<List<Document>> FindCABDocumentsByStatesAsync(State[] states)
        {
            var list = new List<Document>();
            foreach (var state in states)
            {
                var docs = await _cabRepostitory.Query<Document>(d => d.State == state);
                if (docs != null && docs.Any())
                {
                    list.AddRange(docs);
                }
            }

            list = list.OrderByDescending(l => l.CreatedDate).ToList();
            return list;
        }
    }
}
