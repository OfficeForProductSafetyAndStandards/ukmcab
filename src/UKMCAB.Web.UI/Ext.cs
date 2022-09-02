using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace UKMCAB.Web.UI;

public static class Ext
{
    public static IHtmlContent Conditional(this IHtmlHelper htmlHelper, bool condition, string text) => condition ? htmlHelper.Raw(text) : HtmlString.Empty;

    public static IHtmlContent Conditional(this IHtmlHelper htmlHelper, bool condition, HtmlString html) => condition ? html : HtmlString.Empty;

    public static IHtmlContent Conditional<T>(this IHtmlHelper<T> htmlHelper, bool condition, string text) => condition ? htmlHelper.Raw(text) : HtmlString.Empty;

    public static IHtmlContent Conditional<T>(this IHtmlHelper<T> htmlHelper, bool condition, HtmlString html) => condition ? html : HtmlString.Empty;
}
