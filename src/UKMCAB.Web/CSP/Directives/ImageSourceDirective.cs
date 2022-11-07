namespace UKMCAB.Web.CSP.Directives;

public class ImageSourceDirective : Directive
{
    public ImageSourceDirective()
        : this(CspConstants.SelfKeyword)
    {
    }

    public ImageSourceDirective(params string[] values)
        : base(CspConstants.ImageSourceDirectiveKey)
    {
        AddValues(values);
    }
}
