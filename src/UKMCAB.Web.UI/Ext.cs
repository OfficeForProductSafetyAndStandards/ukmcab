using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace UKMCAB.Web.UI;

public static class Ext
{
    public static IHtmlContent Conditional(this IHtmlHelper htmlHelper, bool condition, string text) => condition ? htmlHelper.Raw(text) : HtmlString.Empty;

    public static IHtmlContent Conditional(this IHtmlHelper htmlHelper, bool condition, HtmlString html) => condition ? html : HtmlString.Empty;

    public static IHtmlContent Conditional(this IHtmlHelper htmlHelper, bool condition, string text, string substituteText) => condition ? htmlHelper.Raw(text) : htmlHelper.Raw(substituteText);

    public static IHtmlContent Conditional(this IHtmlHelper htmlHelper, bool condition, HtmlString html, HtmlString substituteHTML) => condition ? html : substituteHTML;

    public static IHtmlContent Conditional<T>(this IHtmlHelper<T> htmlHelper, bool condition, string text) => condition ? htmlHelper.Raw(text) : HtmlString.Empty;

    public static IHtmlContent Conditional<T>(this IHtmlHelper<T> htmlHelper, bool condition, HtmlString html) => condition ? html : HtmlString.Empty;

    public static IHtmlContent SanitiseURL(this IHtmlHelper htmlHelper, string text) => text.StartsWith("http", StringComparison.InvariantCultureIgnoreCase) ? htmlHelper.Raw(text) : htmlHelper.Raw($"https://{text}");

    public static IHtmlContent ShowModelStateFormGroupErrorClass(this IHtmlHelper htmlHelper, ModelStateDictionary modelState, string modelStateKey)
    {
        return ShowErrorClass(htmlHelper, modelState, modelStateKey, "govuk-form-group--error");
    }
    public static IHtmlContent ShowModelStateInputErrorClass(this IHtmlHelper htmlHelper, ModelStateDictionary modelState, string modelStateKey)
    {
        return ShowErrorClass(htmlHelper, modelState, modelStateKey, "govuk-input--error");
    }
    public static IHtmlContent ShowErrorClass(this IHtmlHelper htmlHelper, ModelStateDictionary modelState, string modelStateKey, string errorClass)
    {
        return modelState.Keys.Any(k => k.Equals(modelStateKey)) && modelState[modelStateKey].ValidationState == ModelValidationState.Invalid ?
            htmlHelper.Raw(errorClass) :
            HtmlString.Empty;
    }

    public static IHtmlContent OpensInNewWindow(this IHtmlHelper htmlHelper) =>
        htmlHelper.Raw("<span class=\"govuk-visually-hidden\">(opens in a new window)</span>");

    public static IHtmlContent ValueOrNotProvided(this IHtmlHelper htmlHelper, string text) =>
        string.IsNullOrWhiteSpace(text) ? htmlHelper.Raw(Constants.NotProvided) : htmlHelper.Raw(text);

    public static string ControllerName(this string text) => text.Replace("Controller", string.Empty);
}
