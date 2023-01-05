using UKMCAB.Core.Models;

namespace UKMCAB.Core.Services
{
    public interface ICABRepository
    {
        Task<Document> GetByIdAsync(string id);
        Task<List<Document>> Query(string whereClause);
        Task<Document> CreateAsync(Document document);
    }
}
