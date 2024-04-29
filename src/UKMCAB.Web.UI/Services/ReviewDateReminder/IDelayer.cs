namespace UKMCAB.Web.UI.Services.ReviewDateReminder
{
    public interface IDelayer
    {
        Task Delay(int millisecondsDelay, CancellationToken cancellationToken);
    }
}
