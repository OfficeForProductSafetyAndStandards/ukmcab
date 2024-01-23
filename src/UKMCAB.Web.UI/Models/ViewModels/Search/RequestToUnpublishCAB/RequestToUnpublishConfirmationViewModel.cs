using UKMCAB.Web.UI.Models.ViewModels.Shared;
namespace UKMCAB.Web.UI.Models.ViewModels.Search.RequestToUnpublishCAB;

public class RequestToUnpublishConfirmationViewModel : ConfirmationViewModel
{
    public string URLSlug { get; init; } = string.Empty;
}