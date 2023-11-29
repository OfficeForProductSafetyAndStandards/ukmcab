using Bogus.DataSets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using UKMCAB.Core.Security;
using UKMCAB.Data.CosmosDb.Services.CAB;
using UKMCAB.Data.Models;

namespace UKMCAB.Core.Tests.FakeRepositories
{
    public class FakeCABRepository : ICABRepository
    {
        public List<Document> Documents { get; set; }
        public FakeCABRepository()
        {
            Documents = new List<Document>();
        }
        public Task<Document> CreateAsync(Document document)
        {
            Documents.Add(document);
            return Task.FromResult(document);
        }

        public Task<bool> Delete(Document document)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetCABCountByStatusAsync(Status status)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetCABCountBySubStatusAsync(SubStatus subStatus)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Document> GetItemLinqQueryable()
        {
            throw new NotImplementedException();
        }

        public Task<bool> InitialiseAsync(bool force = false)
        {
            throw new NotImplementedException();
        }

        public Task<List<T>> Query<T>(Expression<Func<T, bool>> predicate)
        {
            var auditLog1 = new Audit { DateTime = DateTime.Now, UserRole = Roles.UKAS.Id }; // UKAS audit log
            var auditLog2 = new Audit { DateTime = DateTime.Now, UserRole = Roles.OPSS.Id }; // OPSS audit log

            var data = new List<Document>
            {
                new Document{AuditLog = new List<Audit>{auditLog1}, StatusValue = Status.Draft },
                new Document{AuditLog = new List<Audit>{auditLog1}, StatusValue = Status.Draft },
                new Document{AuditLog = new List<Audit>{auditLog1}, StatusValue = Status.Archived },
                new Document{AuditLog = new List<Audit>{auditLog2}, StatusValue = Status.Archived }
            };

            return Query<T>(data.Cast<T>().ToList(), predicate);
            //return (List<T>)data;
        }

        private async Task<List<T>> Query<T>(List<T> container, Expression<Func<T, bool>> predicate)
        {
            //var result = new List<T>();
            var result = container.Where( predicate.Compile()).ToList();
            return result.Cast<T>().ToList();

        }

        public Task<bool> Update(Document document)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Document document)
        {
            throw new NotImplementedException();
        }
    }
}
