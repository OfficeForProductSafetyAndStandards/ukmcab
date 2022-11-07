namespace UKMCAB.Web.CSP.Directives;

public class FrameAncestorsDirective : Directive
{
    public FrameAncestorsDirective()
        : this(CspConstants.SelfKeyword)
    {
    }

    public FrameAncestorsDirective(params string[] values)
        : base(CspConstants.FrameAncestorsDirectiveKey)
    {
        AddValues(values);
    }
}
