namespace UKMCAB.Web.CSP.Directives;

public class ScriptSourceDirective : Directive
{
    public ScriptSourceDirective()
        : this(CspConstants.SelfKeyword)
    {
    }

    public ScriptSourceDirective(params string[] values)
        : base(CspConstants.ScriptSourceDirectiveKey)
    {
        AddValues(values);
    }
}
