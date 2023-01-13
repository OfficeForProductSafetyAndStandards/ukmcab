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

        public async Task<Document> CreateCABDocumentAsync(string email, CABData cabData, State state = State.Saved)
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
                State = state
            };
            return await _cabRepostitory.CreateAsync(document);
        }

        public async Task<List<Document>> FindCABDocumentsByNameAsync(string cabName)
        {
            var docs = await _cabRepostitory.Query<Document>(d =>
                d.CABData.Name.Equals(cabName, StringComparison.CurrentCultureIgnoreCase));
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
                if (docs.Any())
                {
                    list.AddRange(docs);
                }
            }
            return list;
        }
    }
}
