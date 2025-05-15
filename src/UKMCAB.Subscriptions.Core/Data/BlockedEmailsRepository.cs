using UKMCAB.Subscriptions.Core.Data.Models;
using UKMCAB.Subscriptions.Core.Domain;

namespace UKMCAB.Subscriptions.Core.Data;

public interface IBlockedEmailsRepository : IRepository
{
    Task BlockAsync(EmailAddress emailAddress);
    Task<bool> IsBlockedAsync(EmailAddress emailAddress);
    Task UnblockAsync(EmailAddress emailAddress);
}

public class BlockedEmailsRepository : Repository<TableEntity>, IBlockedEmailsRepository
{
    private const string tabkeKey = $"{SubscriptionsCoreServicesOptions.TableNamePrefix}blockedemail";
    public BlockedEmailsRepository(SubscriptionDbContext dataContext) : base(dataContext, tabkeKey) { }
    public async Task<bool> IsBlockedAsync(EmailAddress emailAddress) => await GetAsync<TableEntity>(new Keys(tabkeKey, string.Empty, emailAddress)) != null;
    public async Task BlockAsync(EmailAddress emailAddress) => await UpsertAsync(new TableEntity(tabkeKey, string.Empty, emailAddress));
    public async Task UnblockAsync(EmailAddress emailAddress) => await DeleteAsync(new Keys(tabkeKey, string.Empty, emailAddress));
}
