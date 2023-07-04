using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace UKMCAB.Web.TagHelpers
{
    [HtmlTargetElement(Attributes = ErrorDescribedByName)]
    public class ErrorDescribedByTagHelper : TagHelper
    {
        private const string ErrorDescribedByName = "error-describedby";

        private const string AriaDescribedByAttribute = "aria-describedby";

        [HtmlAttributeName(ErrorDescribedByName)]
        public string Name { get; set; }


        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ViewContext.ViewData.ModelState.TryGetValue(Name, out ModelStateEntry entry);
            if (entry != null && entry.Errors.Any())
            {
                var errorID = $"{Name.ToLower()}-error";
                if (output.Attributes.ContainsName(AriaDescribedByAttribute))
                {
                    var currentValues = output.Attributes[AriaDescribedByAttribute].Value;
                    var newClassAttribute = new TagHelperAttribute(
                        AriaDescribedByAttribute,
                        new HtmlString($"{currentValues} {errorID}"));

                    output.Attributes.SetAttribute(newClassAttribute);
                }
                else
                {
                    output.Attributes.Add(AriaDescribedByAttribute, errorID);
                }
            }
        }
    }
}
