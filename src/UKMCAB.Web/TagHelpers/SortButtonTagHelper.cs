using Microsoft.AspNetCore.Razor.TagHelpers;
using UKMCAB.Common;

namespace UKMCAB.Web.TagHelpers;

[HtmlTargetElement("sort")]
public class SortButtonTagHelper : TagHelper
{
    /// <summary>
    /// Name of the current sort direction
    /// </summary>
    [HtmlAttributeName("sort-direction")]
    public string? SortDirection { get; set; }

    /// <summary>
    /// Name of the current sorted field
    /// </summary>
    [HtmlAttributeName("sort")]
    public string? SortField { get; set; }

    /// <summary>
    /// Name of the target field by which to sort
    /// </summary>
    [HtmlAttributeName("target")]
    public string? TargetField { get; set; }
    
    /// <summary>
    /// Name of the tab to show
    /// </summary>
    [HtmlAttributeName("tab-to-show")]
    public string? TabToShow { get; set; }

    /// <summary>
    /// Whether to append the tab name using a clientside anchor tag (#TabToShow) or add a serverside variable (&tabName=TabToShow).
    /// </summary>
    [HtmlAttributeName("use-serverside-tabs")]
    public bool UseServerSideTabs { get; set; } = false;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "a";
        output.Attributes.EnsureAttribute("role", "button");
        output.Attributes.EnsureCssClass("sort-button");
        output.Attributes.EnsureAttribute("aria-hidden", "true");
        var text = output.PostContent.GetContent();


        if(SortField == TargetField)
        {
            output.Attributes.EnsureAttribute("href", $"?sf={TargetField}&sd={SortDirectionHelper.Opposite(SortDirection)}{(UseServerSideTabs ? "&TabName=" + TabToShow : "#" + TabToShow)}");
            output.Attributes.EnsureAttribute("aria-sort", SortDirectionHelper.Get(SortDirection));
            
            output.PostContent.AppendHtml($"<span class='govuk-visually-hidden'>{text}, sorted {SortDirectionHelper.GetFriendly(SortDirection)} press enter or space to change the sorting order.</span>");
        }
        else
        {
            output.Attributes.EnsureAttribute("href", $"?sf={TargetField}&sd={SortDirectionHelper.Ascending}{(UseServerSideTabs ? "&TabName=" + TabToShow : "#" + TabToShow)}");
            output.Attributes.EnsureAttribute("aria-sort", "none");
            output.PostContent.AppendHtml($"<span class='govuk-visually-hidden'>{text}, Not sorted, Press enter or space to sort</span>");
        }

    }
}
