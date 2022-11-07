namespace UKMCAB.Web.CSP.Directives;

public class FormActionDirective : Directive
{
    public FormActionDirective()
        : this(CspConstants.SelfKeyword)
    {
    }

    public FormActionDirective(params string[] values)
        : base(CspConstants.FormActionDirectiveKey)
    {
        AddValues(values);
    }
}
