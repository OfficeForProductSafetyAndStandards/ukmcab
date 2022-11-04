namespace UKMCAB.Web.CSP.Directives;

public class MediaSourceDirective : Directive
{
    public MediaSourceDirective()
        : this(CspConstants.SelfKeyword)
    {
    }

    public MediaSourceDirective(params string[] values)
        : base(CspConstants.MediaSourceDirectiveKey)
    {
        AddValues(values);
    }
}
