namespace UKMCAB.Web.UI.Models.ViewModels;

public class MessageViewModel : ILayoutModel
{
    public string? Title { get; set; }
    public string? Heading { get; set; }
    public string? Body { get; set; }
    public string? LinkLabel { get; set; }
    public string? LinkUrl { get; set; }
    public string? ViewName { get; set; }
}
