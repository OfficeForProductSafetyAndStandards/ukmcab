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

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "a";
        output.Attributes.EnsureAttribute("role", "button");
        output.Attributes.EnsureCssClass("sort-button");
        
        if(SortField == TargetField)
        {
            output.Attributes.EnsureAttribute("href", $"?sf={TargetField}&sd={SortDirectionHelper.Opposite(SortDirection)}");
            output.Attributes.EnsureAttribute("aria-sort", SortDirectionHelper.Get(SortDirection));
        }
        else
        {
            output.Attributes.EnsureAttribute("href", $"?sf={TargetField}&sd={SortDirectionHelper.Ascending}");
            output.Attributes.EnsureAttribute("aria-sort", "none");
        }
    }
}
