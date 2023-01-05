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

        public async Task<Document> CreateCABDocumentAsync(string email, CABData cabData)
        {
            var documents = await FindCABDocumentsAsync(cabData.Name);

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
                State = State.Saved
            };
            return await _cabRepostitory.CreateAsync(document);
        }

        public async Task<List<Document>> FindCABDocumentsAsync(string cabName)
        {
            var documents = await _cabRepostitory.Query($"c.CABData.Name = \"{cabName}\"");
            return documents;
        }
    }
}
