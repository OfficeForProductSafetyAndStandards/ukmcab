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

        public async Task<bool> DocumentWithKeyIdentifiersExists(Document document)
        {
            var documents = await _cabRepostitory.Query<Document>(d =>
                d.Name.Equals(document.Name, StringComparison.InvariantCultureIgnoreCase) ||
                d.CABNumber.Equals(document.CABNumber)
            );
            return documents.Any();
        }

        public async Task<Document> FindDocumentByCABIdAsync(string id)
        {
            var doc = await _cabRepostitory.Query<Document>(d => d.IsPublished && d.CABId.Equals(id));
            return doc.Any() && doc.Count == 1 ? doc.First() : null;
        }

        public async Task<Document> CreateDocumentAsync(string userEmail, Document document)
        {
            var documentExists = await DocumentWithKeyIdentifiersExists(document);

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

        //public async Task<List<Document>> FindCABDocumentsByStatesAsync(State[] states)
        //{
        //    var list = new List<Document>();
        //    foreach (var state in states)
        //    {
        //        var docs = await _cabRepostitory.Query<Document>(d => d.State == state);
        //        if (docs != null && docs.Any())
        //        {
        //            list.AddRange(docs);
        //        }
        //    }

        //    list = list.OrderByDescending(l => l.CreatedDate).ToList();
        //    return list;
        //}
    }
}
