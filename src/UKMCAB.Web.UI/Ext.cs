using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Linq.Expressions;
using System.Security.Claims;

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

    public static IHtmlContent ValidationCssFor<TModel, TProperty>(
            this IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression, string cssClass)
    {
        var error = (TagBuilder)htmlHelper.ValidationMessageFor(expression);
        return !error.HasInnerHtml
            ? HtmlString.Empty : 
            (IHtmlContent)new HtmlString(cssClass);
    }

    public static IHtmlContent CssFor<TModel, TProperty>(
            this IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression, string mainCssClass, string errorCssState = "--error")
    {
        var error = (TagBuilder) htmlHelper.ValidationMessageFor(expression);
        var css = !error.HasInnerHtml
            ? mainCssClass :
            $"{mainCssClass} {mainCssClass}{errorCssState}";
        return new HtmlString(css);
    }

    public static IHtmlContent ShowModelStateFormGroupErrorClass(this IHtmlHelper htmlHelper, ModelStateDictionary modelState, string modelStateKey)
    {
        return ShowErrorClass(htmlHelper, modelState, modelStateKey, "govuk-form-group--error");
    }
    public static IHtmlContent ShowModelStateInputErrorClass(this IHtmlHelper htmlHelper, ModelStateDictionary modelState, string modelStateKey)
    {
        return ShowErrorClass(htmlHelper, modelState, modelStateKey, "govuk-input--error");
    }
    public static IHtmlContent ShowModelStateSelectErrorClass(this IHtmlHelper htmlHelper, ModelStateDictionary modelState, string modelStateKey)
    {
        return ShowErrorClass(htmlHelper, modelState, modelStateKey, "govuk-select--error");
    }

    public static IHtmlContent ShowErrorClass(this IHtmlHelper htmlHelper, ModelStateDictionary modelState, string modelStateKey, string errorClass)
    {
        return modelState.Keys.Any(k => k.Equals(modelStateKey)) && modelState[modelStateKey].ValidationState == ModelValidationState.Invalid ?
            htmlHelper.Raw(errorClass) :
            HtmlString.Empty;
    }

    public static IHtmlContent OpensInNewWindow(this IHtmlHelper htmlHelper) =>
        htmlHelper.Raw("<span class=\"govuk-visually-hidden\">(opens in a new tab)</span>");

    public static IHtmlContent ValueOrNotProvided(this IHtmlHelper htmlHelper, string? text) =>
        string.IsNullOrWhiteSpace(text) ? htmlHelper.Raw(Constants.NotProvided) : htmlHelper.Raw(text);
    public static IHtmlContent ValueOrNone(this IHtmlHelper htmlHelper, string? text) =>
        string.IsNullOrWhiteSpace(text) ? htmlHelper.Raw(Constants.None) : htmlHelper.Raw(text);

    public static string ControllerName(this string text) => text.Replace("Controller", string.Empty);

    public static string Sentenceify(this IEnumerable<string>? list, string? fallback = null)
    {
        if (list != null && list.Any())
        {
            var len = list.Count();
            if (len == 1)
            {
                return list.First();
            }
            else
            {
                return $"{list.First()} and {len - 1} other{(len > 2 ? "s" : string.Empty)}";
            }
        }
        return fallback ?? string.Empty;
    }

    public static string FormatTitle(this string title, bool IsValidModelState)
    {
        var titleComponents = new List<string>();
        if (!IsValidModelState)
        {
            titleComponents.Add("Error");
        }

        if (title != null && !string.IsNullOrWhiteSpace(title))
        {
            titleComponents.Add(title);
        }
        titleComponents.Add(Constants.SiteName);
        return string.Join(" - ", titleComponents);
    }

    public static bool HasClaim(this ClaimsPrincipal principal, string type) => principal.HasClaim(x => x.Type == type);
}
