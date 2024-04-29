
namespace UKMCAB.Web.UI.Services.ReviewDateReminder
{
    public class Delayer : IDelayer
    {
        public Task Delay(int millisecondsDelay, CancellationToken cancellationToken)
        {
            return  Task.Delay(millisecondsDelay, cancellationToken);
        }
    }
}
