using System;

namespace UKMCAB.Web.CSP.Directives;

public class Sha256Directive : Directive
{
    public Sha256Directive()
        : this(CspConstants.SelfKeyword)
    {
    }

    public Sha256Directive(params string[] values)
        : base(CspConstants.ScriptSourceDirectiveKey)
    {
        foreach (var sha in values)
        {
            AddValue($"'{CspConstants.Sha256Prefix}{sha}'");
        }
    }

    public Sha256Directive(Func<string> o)
        : base(CspConstants.ScriptSourceDirectiveKey)
    {
        var sha = o();
        AddValue($"'{CspConstants.Sha256Prefix}{sha}'");
    }
}
