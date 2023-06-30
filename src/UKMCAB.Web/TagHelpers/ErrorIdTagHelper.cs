using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace UKMCAB.Web.TagHelpers
{
    [HtmlTargetElement(Attributes = ErrorIDName)]
    public class ErrorIdTagHelper : TagHelper
    {
        private const string ErrorIDName = "error-id";

        [HtmlAttributeName(ErrorIDName)]
        public string Name { get; set; }


        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ViewContext.ViewData.ModelState.TryGetValue(Name, out ModelStateEntry entry);
            if (entry != null && entry.Errors.Any())
            {
                output.Attributes.Add("id", $"{Name.ToLower()}-error");
            }
        }
    }
}
