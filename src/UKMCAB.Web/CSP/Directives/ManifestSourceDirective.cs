namespace UKMCAB.Web.CSP.Directives;

public class ManifestSourceDirective : Directive
{
    public ManifestSourceDirective()
        : this(CspConstants.SelfKeyword)
    {
    }

    public ManifestSourceDirective(params string[] values)
        : base(CspConstants.ManifestSourceDirectiveKey)
    {
        AddValues(values);
    }
}
