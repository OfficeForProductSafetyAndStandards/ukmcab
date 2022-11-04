namespace UKMCAB.Web.CSP.Directives;

public class BaseUriDirective : Directive
{
    public BaseUriDirective()
        : this(CspConstants.SelfKeyword)
    {
    }

    public BaseUriDirective(params string[] values)
        : base(CspConstants.BaseUriDirectiveKey)
    {
        AddValues(values);
    }
}
