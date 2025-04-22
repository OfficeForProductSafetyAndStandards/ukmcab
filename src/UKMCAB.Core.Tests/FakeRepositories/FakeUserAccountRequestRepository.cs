using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UKMCAB.Data.Domain;
using UKMCAB.Data.Interfaces.Services.User;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Core.Tests.FakeRepositories
{
    public class FakeUserAccountRequestRepository : IUserAccountRequestRepository
    {
        public List<UserAccountRequest> UserAccountRequests { get; set; } = new();

        public void SeedData()
        {
            for (int i = 0; i < 5; i++)
            {
                var inceptionDate = DateTime.UtcNow.AddDays(-(i * 10));
                UserAccountRequests.Add(new UserAccountRequest
                {
                    Id = $"{i}",
                    SubjectId = $"Subject-{i}",
                    FirstName = $"Test{i}",
                    Surname = $"Surname{i}",
                    Comments = $"Test{i} comments",
                    CreatedUtc = inceptionDate,
                    EmailAddress = $"test{i}@test.com",
                    ContactEmailAddress = $"contactemail{i}@test.com",
                    LastUpdatedUtc = inceptionDate,
                    OrganisationName = $"Organisation{i}",
                    Status = UserAccountRequestStatus.Pending,
                    AuditLog = new List<Audit>
                    {
                        new()
                        {
                            DateTime = inceptionDate,
                            UserName = $"Test{i} Surname{i}",
                            Action = AuditUserActions.UserAccountRequest,
                            Comment = $"Test{i} comments"
                        }
                    }
                });
            }

            var id = "rejected-user";

            UserAccountRequests.Add(new UserAccountRequest
            {
                Id = $"{id}",
                SubjectId = $"Subject-{id}",
                FirstName = $"Test{id}",
                Surname = $"Surname{id}",
                Comments = $"Test{id} comments",
                CreatedUtc = DateTime.UtcNow.AddDays(-10),
                EmailAddress = $"test-{id}@test.com",
                ContactEmailAddress = $"contactemail-{id}@test.com",
                LastUpdatedUtc = DateTime.UtcNow.AddDays(-5),
                OrganisationName = $"Organisation{id}",
                Status = UserAccountRequestStatus.Rejected,
                AuditLog = new List<Audit>
                {
                    new()
                    {
                        DateTime = DateTime.UtcNow.AddDays(-10),
                        UserName = $"Test{id} Surname{id}",
                        Action = AuditUserActions.UserAccountRequest,
                        Comment = $"Test{id} comments"
                    },
                    new()
                    {
                        DateTime = DateTime.UtcNow.AddDays(-5),
                        UserName = "Admin User",
                        UserId = "100",
                        UserRole = "OPSS",
                        Action = AuditUserActions.DeclineAccountRequest,
                        Comment = $"Admin user comments"
                    }
                }
            });

        }

        public Task CreateAsync(UserAccountRequest userAccount)
        {
            userAccount.Id = Guid.NewGuid().ToString();
            UserAccountRequests.Add(userAccount);
            return Task.CompletedTask;
        }

        public Task<UserAccountRequest?> GetPendingAsync(string subjectId)
        {
            var userAccountRequest = UserAccountRequests.SingleOrDefault(uar => uar.SubjectId == subjectId);
            return Task.FromResult(userAccountRequest);
        }

        public Task<IEnumerable<UserAccountRequest>> ListAsync(UserAccountRequestListOptions options)
        {
            return Task.FromResult<IEnumerable<UserAccountRequest>>(UserAccountRequests.Where(uar => uar.Status == options.Status).Skip(options.SkipTake.Skip).Take(options.SkipTake.Take).ToList());
        }
        public Task<int> CountAsync(UserAccountRequestStatus? status = null)
        {
            return Task.FromResult(UserAccountRequests.Where(uar => uar.Status == status).Count());
        }

        public Task UpdateAsync(UserAccountRequest userAccount)
        {
            var existingAccount = UserAccountRequests.SingleOrDefault(uar => uar.Id == userAccount.Id);
            if (existingAccount != null)
            {
                UserAccountRequests.Remove(existingAccount);
            }
            UserAccountRequests.Add(userAccount);
            return Task.CompletedTask;
        }

        public Task InitialiseAsync()
        {
            return Task.CompletedTask;
        }

        public Task<UserAccountRequest?> GetAsync(string id)
        {
            var userAccountRequest = UserAccountRequests.SingleOrDefault(uar => uar.Id == id);
            return Task.FromResult(userAccountRequest);
        }

    }
}
