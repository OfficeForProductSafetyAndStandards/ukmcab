using System.Linq.Expressions;
using Microsoft.Azure.Cosmos.Linq;
using UKMCAB.Data.Interfaces.Services.CAB;
using UKMCAB.Data.Models;

namespace UKMCAB.Data.PostgreSQL.Services.CAB;

public class PostgreCABRepository : ICABRepository
{
    private readonly ApplicationDataContext _dbContext;

    public PostgreCABRepository(ApplicationDataContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Document> CreateAsync(Document document, DateTime lastUpdatedDateTime)
    {
        document.id = Guid.NewGuid().ToString();
        document.Version = DataConstants.Version.Number;
        document.LastUpdatedDate = lastUpdatedDateTime;

        var blob = new CABDocumentBlob(document);

        var doc = _dbContext.Documents.Add(blob);
        await _dbContext.SaveChangesAsync();

        return doc.Entity.CabBlob;
    }

    public async Task<bool> DeleteAsync(Document document)
    {
        var data = _dbContext.Documents.FirstOrDefault(i => i.id == document.id);
        _dbContext.Documents.Remove(data);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<int> GetCABCountByStatusAsync(Status status)
    {
        var list = _dbContext.Documents.AsQueryable();
        if (status == Status.Unknown)
        {
            return await list.CountAsync();
        }

        return await list.Where(x => x.StatusValue == status).CountAsync();
    }

    public async Task<int> GetCABCountBySubStatusAsync(SubStatus subStatus)
    {
        var list = _dbContext.Documents.AsQueryable();
        if (subStatus == SubStatus.None)
        {
            return await list.CountAsync();
        }

        return await list.Where(x => x.SubStatus == subStatus).CountAsync();
    }

    public IQueryable<Document> GetItemLinqQueryable()
    {
        return _dbContext.Documents.Select(d => d.CabBlob).AsQueryable();
    }

    public Task<bool> InitialiseAsync(bool force = false)
    {
        return Task.FromResult(force);
    }

    private async Task<List<Document>> PrivateQuery<T>(Expression<Func<CABDocumentBlob, bool>> predicate)
    {
        var data = _dbContext.Documents.Where(predicate);
        return data.Select(d => d.CabBlob).ToList();
    }

    public async Task<List<T>> Query<T>(Expression<Func<T, bool>> predicate)
    {
        if (typeof(T) == typeof(Document))
        {
            // Projection from DocumentPart to Document
            Expression<Func<CABDocumentBlob, Document>> projection = p => new Document
            {
                id = p.id,
                StatusValue = p.StatusValue,
                CABId = p.CABId,
                SubStatus = p.SubStatus,
                CreatedByUserGroup = p.CreatedByUserGroup,
                URLSlug = p.URLSlug,
                Name = p.Name,
                UKASReference = p.UKASReference,
                CABNumber = p.CABNumber,
                Version = p.Version
            };

            // Convert the Document predicate to work on DocumentPart
            var docPredicate = predicate as Expression<Func<Document, bool>>;

            if (docPredicate == null)
                throw new ArgumentException("Predicate must be of type Expression<Func<Document, bool>>");

            var mappedPredicate = PredicateMapper.Map(docPredicate, projection); 

            // Run query on DocumentPart
            var ids = _dbContext.Set<CABDocumentBlob>()
                .Where(mappedPredicate)
                .Select(p => p.id)
                .ToList();

            // Load full Documents
            var fullDocs = _dbContext.Set<Document>()
                .Where(d => ids.Contains(d.id))
                .ToList();

            return fullDocs.Cast<T>().ToList();
        }

        throw new NotSupportedException("Only Document queries are supported");
    }

    public async Task UpdateAsync(Document document, DateTime? lastUpdatedDate = default)
    {
        var data = _dbContext.Documents.FirstOrDefault(i => i.id == document.id);

        document.LastUpdatedDate = lastUpdatedDate ?? DateTime.Now;
        data.Update(document);

        _dbContext.Documents.Update(data);

        await _dbContext.SaveChangesAsync();
    }
}

public static class PredicateMapper
{
    public static Expression<Func<TTarget, bool>> Map<TSource, TTarget>(
        Expression<Func<TSource, bool>> sourcePredicate,
        Expression<Func<TTarget, TSource>> projection)
    {
        var param = Expression.Parameter(typeof(TTarget), "target");
        var body = new ReplaceParamVisitor(sourcePredicate.Parameters[0], Expression.Invoke(projection, param))
            .Visit(sourcePredicate.Body);

        return Expression.Lambda<Func<TTarget, bool>>(body!, param);
    }

    private class ReplaceParamVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParam;
        private readonly Expression _newExpr;

        public ReplaceParamVisitor(ParameterExpression oldParam, Expression newExpr)
        {
            _oldParam = oldParam;
            _newExpr = newExpr;
        }

        protected override Expression VisitParameter(ParameterExpression node) =>
            node == _oldParam ? _newExpr : base.VisitParameter(node);
    }
}
