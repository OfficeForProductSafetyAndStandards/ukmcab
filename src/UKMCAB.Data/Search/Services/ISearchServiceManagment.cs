namespace UKMCAB.Data.Search.Services
{
    public interface ISearchServiceManagment
    {
        Task InitialiseAsync(bool force = false);
    }
}
