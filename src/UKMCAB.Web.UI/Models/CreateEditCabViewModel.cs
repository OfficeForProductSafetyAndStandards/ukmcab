using UKMCAB.Data.CosmosDb.Models;

namespace UKMCAB.Web.UI.Models;

public class CreateEditCabViewModel : ILayoutModel
{
    public CAB Data { get; set; }
    public string EditUrlTemplate { get; set; }

    string? ILayoutModel.Title => "Create/edit CAB";
}