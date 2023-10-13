using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace UKMCAB.Web.TagHelpers
{
    [HtmlTargetElement(Attributes = ErrorIDForName)]
    public class ErrorIdTagHelper : TagHelper
    {
        private const string ErrorIDForName = "error-id-for";

        [HtmlAttributeName(ErrorIDForName)]
        public ModelExpression For { get; set; }


        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ViewContext.ViewData.ModelState.TryGetValue(For.Name, out ModelStateEntry entry);
            if (entry != null && entry.Errors.Any())
            {
                output.Attributes.Add("id", $"{For.Name.ToLower()}-error");
            }
            else
            {
                var existingClass = output.Attributes.FirstOrDefault(f => f.Name == "class");
                var cssClass = string.Empty;
                if (existingClass != null)
                {
                    cssClass = existingClass.Value.ToString();
                }
                cssClass = cssClass + " app-no-display";
                var ta = new TagHelperAttribute("class", cssClass);
                output.Attributes.Remove(existingClass);
                output.Attributes.Add(ta);
            }
        }
    }
}
