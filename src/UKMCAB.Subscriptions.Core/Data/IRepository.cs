namespace UKMCAB.Subscriptions.Core.Data;

public interface IRepository
{
    Task DeleteAllAsync();
    Task DeleteAsync(Keys keys);
    Task<bool> ExistsAsync(Keys keys);
}

