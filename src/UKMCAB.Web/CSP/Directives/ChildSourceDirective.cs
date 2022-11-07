namespace UKMCAB.Web.CSP.Directives;

public class ChildSourceDirective : Directive
{
    public ChildSourceDirective()
        : this(CspConstants.SelfKeyword)
    {
    }

    public ChildSourceDirective(params string[] values)
        : base(CspConstants.ChildSourceDirectiveKey)
    {
        AddValues(values);
    }
}
