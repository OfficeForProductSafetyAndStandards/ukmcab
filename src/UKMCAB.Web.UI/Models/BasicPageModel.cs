namespace UKMCAB.Web.UI.Models;

public record BasicPageModel(string? Title = null) : ILayoutModel
{
    public string? Title { get; set; } = Title;
}