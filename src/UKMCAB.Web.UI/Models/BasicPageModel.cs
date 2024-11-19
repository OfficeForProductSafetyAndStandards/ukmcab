namespace UKMCAB.Web.UI.Models;

public record BasicPageModel(string? Title = null, string? SubTitle = null) : ILayoutModel
{
    public string? Title { get; set; } = Title;
    public string? SubTitle { get; set; } = SubTitle;
}