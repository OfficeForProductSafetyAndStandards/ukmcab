namespace UKMCAB.Web.CSP.Directives;

public class DefaultSourceDirective : Directive
{
    public DefaultSourceDirective()
        : this(CspConstants.SelfKeyword)
    {
    }

    public DefaultSourceDirective(params string[] values)
        : base(CspConstants.DefaultSourceDirectiveKey)
    {
        AddValues(values);
    }
}
