namespace UKMCAB.Web.CSP.Directives;

public class StyleSourceDirective : Directive
{
    public StyleSourceDirective()
        : this(CspConstants.SelfKeyword)
    {
    }

    public StyleSourceDirective(params string[] values)
        : base(CspConstants.StyleSourceDirectiveKey)
    {
        AddValues(values);
    }
}
