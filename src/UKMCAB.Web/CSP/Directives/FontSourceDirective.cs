namespace UKMCAB.Web.CSP.Directives;

public class FontSourceDirective : Directive
{
    public FontSourceDirective()
        : this(CspConstants.SelfKeyword)
    {
    }

    public FontSourceDirective(params string[] values)
        : base(CspConstants.FontSourceDirectiveKey)
    {
        AddValues(values);
    }
}
