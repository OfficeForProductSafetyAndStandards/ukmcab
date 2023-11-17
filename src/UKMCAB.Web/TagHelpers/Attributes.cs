using Microsoft.AspNetCore.Razor.TagHelpers;
using UKMCAB.Common;

namespace UKMCAB.Web.TagHelpers;

public static class Attributes
{
    public static void EnsureCssClass(this TagHelperAttributeList attributes, string cssClassName)
    {
        var @class = attributes.FirstOrDefault(x => x.Name.DoesEqual("class"));
        if (@class == null)
        {
            @class = new TagHelperAttribute("class", cssClassName);
            attributes.Add(@class);
        }
        else
        {
            if (@class.Value.ToString().DoesNotContain(cssClassName))
            {
                var value = $"{@class.Value} {cssClassName}";
                attributes.Remove(@class);
                attributes.Add(new TagHelperAttribute("class", value));
            }
        }
    }

    public static void EnsureAttribute(this TagHelperAttributeList attributes, string name, string value)
    {
        var @class = attributes.FirstOrDefault(x => x.Name.DoesEqual(name));
        
        if (@class != null)
        {
            attributes.Remove(@class);
        }

        attributes.Add(new TagHelperAttribute(name, value));
    }
}