namespace UKMCAB.Web.CSP.Directives;

public class ObjectSourceDirective : Directive
{
    public ObjectSourceDirective()
        : this(CspConstants.SelfKeyword)
    {
    }

    public ObjectSourceDirective(params string[] values)
        : base(CspConstants.ObjectSourceDirectiveKey)
    {
        AddValues(values);
    }
}
