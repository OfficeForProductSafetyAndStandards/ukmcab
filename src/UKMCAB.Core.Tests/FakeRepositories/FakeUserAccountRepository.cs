using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UKMCAB.Common;
using UKMCAB.Data.Domain;
using UKMCAB.Data.Interfaces.Services.User;
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Core.Tests.FakeRepositories
{
    public class FakeUserAccountRepository : IUserAccountRepository
    {
        public List<UserAccount> UserAccounts;

        public FakeUserAccountRepository()
        {
            UserAccounts = new List<UserAccount>();
        }

        public Task CreateAsync(UserAccount userAccount)
        {
            UserAccounts.Add(userAccount);
            return Task.CompletedTask;
        }

        public async Task<UserAccount?> GetAsync(string id)
        {
            var userAccounts = UserAccounts.SingleOrDefault(uar => uar.Id == id);
            return userAccounts;
        }

        public Task InitialiseAsync()
        {
            return Task.CompletedTask;
        }

        public async Task<int> UserCountAsync(UserAccountLockReason? lockReason = null, bool locked = false)
        {
            if (lockReason == null)
            {
                return UserAccounts.Count(x => x.IsLocked == locked);
            }
            else
            {
                return UserAccounts.Count(x => x.IsLocked == locked && x.LockReason == (UserAccountLockReason)lockReason);
            }
        }

        public async Task<IEnumerable<UserAccount>> ListAsync(UserAccountListOptions options)
        {
            var q = UserAccounts.AsQueryable();

            if (options.ExcludeId.IsNotNullOrEmpty())
            {
                q = q.Where(x => x.Id != options.ExcludeId);
            }

            if (options.IsLocked.HasValue)
            {
                q = q.Where(x => x.IsLocked == options.IsLocked);
                if (options.IsLocked.Value && options.LockReason != null)
                {
                    q = q.Where(x => x.LockReason == options.LockReason);
                }
            }


            var data = q.OrderBy(ua => ua.Surname)
                .Skip(options.SkipTake.Skip)
                .Take(options.SkipTake.Take)
                .ToList();

            return data;
        }

        public async Task PatchAsync<T>(string id, string fieldName, T value)
        {
            var userAccount = await GetAsync(id);
            if (value is DateTime?)
            {
                var dateTime = value as DateTime?;
                userAccount.LastLogonUtc = dateTime;
            }

            await UpdateAsync(userAccount);
        }

        public Task UpdateAsync(UserAccount userAccount)
        {
            var existingAccount = UserAccounts.SingleOrDefault(uar => uar.Id == userAccount.Id);
            if (existingAccount != null)
            {
                UserAccounts.Remove(existingAccount);
            }
            UserAccounts.Add(userAccount);
            return Task.CompletedTask;
        }
    }
}
