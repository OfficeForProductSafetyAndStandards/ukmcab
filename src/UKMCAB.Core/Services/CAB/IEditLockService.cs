namespace UKMCAB.Core.Services;

public interface IEditLockService
{
    public bool Exists(string cabId,string userId);
    public void Set(string cabId,string userId);
    public void Remove(string cabId);
}