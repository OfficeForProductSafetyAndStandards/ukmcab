using System;

namespace UKMCAB.Web.CSP.Directives;

public class NonceDirective : Directive
{
    public NonceDirective()
        : this(CspConstants.SelfKeyword)
    {
    }

    public NonceDirective(params string[] values)
        : base(CspConstants.ScriptSourceDirectiveKey)
    {
        foreach (var nonce in values)
        {
            AddValue($"'{CspConstants.NoncePrefix}{nonce}'");
        }
    }

    public NonceDirective(Func<string> o)
        : base(CspConstants.ScriptSourceDirectiveKey)
    {
        var nonce = o();
        AddValue($"'{CspConstants.NoncePrefix}{nonce}'");
    }
}
