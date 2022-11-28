using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace UKMCAB.Web.UI;

public static class Ext
{
    public static IHtmlContent Conditional(this IHtmlHelper htmlHelper, bool condition, string text) => condition ? htmlHelper.Raw(text) : HtmlString.Empty;

    public static IHtmlContent Conditional(this IHtmlHelper htmlHelper, bool condition, HtmlString html) => condition ? html : HtmlString.Empty;

    public static IHtmlContent Conditional<T>(this IHtmlHelper<T> htmlHelper, bool condition, string text) => condition ? htmlHelper.Raw(text) : HtmlString.Empty;

    public static IHtmlContent Conditional<T>(this IHtmlHelper<T> htmlHelper, bool condition, HtmlString html) => condition ? html : HtmlString.Empty;

    public static IHtmlContent ShowModelStateErrorClass(this IHtmlHelper htmlHelper, ModelStateDictionary modelState,
        string modelStateKey)
    {
        return modelState.Keys.Any(k => k.Equals(modelStateKey)) && modelState[modelStateKey].ValidationState == ModelValidationState.Invalid ? 
            htmlHelper.Raw("govuk-form-group--error") : 
            HtmlString.Empty;
    }
}
