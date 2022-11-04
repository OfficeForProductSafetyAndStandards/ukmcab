namespace UKMCAB.Web.CSP.Directives;

public class ConnectSourceDirective : Directive
{
    public ConnectSourceDirective()
        : this(CspConstants.SelfKeyword)
    {
    }

    public ConnectSourceDirective(params string[] values)
        : base(CspConstants.ConnectSourceDirectiveKey)
    {
        AddValues(values);
    }
}
