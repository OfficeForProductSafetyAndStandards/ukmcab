namespace UKMCAB.Data.Search.Services
{
    public class PostgreSearchServiceManagment : ISearchServiceManagment
    {
        public Task InitialiseAsync(bool force = false)
        {
            return Task.CompletedTask;
        }
    }
}
