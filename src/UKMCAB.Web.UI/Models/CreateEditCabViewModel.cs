using UKMCAB.Data.CosmosDb.Models;

namespace UKMCAB.Web.UI.Models;

public class CreateEditCabViewModel
{
    public CAB Data { get; set; }
    public string EditUrlTemplate { get; set; }
}