using UKMCAB.Web.CSP.Directives;

namespace UKMCAB.Web.CSP;

public static class CspHeaderExtensions
{
    public static CspHeader Add(this CspHeader header, BaseUriDirective directive) => header.Add(directive);

    public static CspHeader Add(this CspHeader header, ChildSourceDirective directive) => header.Add(directive);

    public static CspHeader Add(this CspHeader header, ConnectSourceDirective directive) => header.Add(directive);

    public static CspHeader Add(this CspHeader header, DefaultSourceDirective directive) => header.Add(directive);

    public static CspHeader Add(this CspHeader header, FontSourceDirective directive) => header.Add(directive);

    public static CspHeader Add(this CspHeader header, FormActionDirective directive) => header.Add(directive);

    public static CspHeader Add(this CspHeader header, FrameAncestorsDirective directive) => header.Add(directive);

    public static CspHeader Add(this CspHeader header, ImageSourceDirective directive) => header.Add(directive);

    public static CspHeader Add(this CspHeader header, ManifestSourceDirective directive) => header.Add(directive);

    public static CspHeader Add(this CspHeader header, MediaSourceDirective directive) => header.Add(directive);

    public static CspHeader Add(this CspHeader header, NonceDirective directive) => header.Add(directive);

    public static CspHeader Add(this CspHeader header, ObjectSourceDirective directive) => header.Add(directive);

    public static CspHeader Add(this CspHeader header, ScriptSourceDirective directive) => header.Add(directive);

    public static CspHeader Add(this CspHeader header, Sha256Directive directive) => header.Add(directive);

    public static CspHeader Add(this CspHeader header, StyleSourceDirective directive) => header.Add(directive);

    public static CspHeader Add(this CspHeader header, UpgradeInsecureRequestsDirective directive) => header.Add(directive);

    public static CspHeader AddDefaultCspDirectives(this CspHeader header)
    {
        return header
            .Add(new DefaultSourceDirective(CspConstants.NoneKeyword))
            .Add(new ScriptSourceDirective(CspConstants.SelfKeyword))
            .Add(new ConnectSourceDirective(CspConstants.SelfKeyword))
            .Add(new ImageSourceDirective(CspConstants.SelfKeyword))
            .Add(new StyleSourceDirective(CspConstants.SelfKeyword));
    }
    public static CspHeader AddScriptNonce(this CspHeader header, string nonce) => header.Add(new NonceDirective(nonce));

    public static CspHeader AddScriptNonce(this CspHeader header, Func<string> nonce) => header.Add(new NonceDirective(nonce));

    public static CspHeader AddScriptSha256(this CspHeader header, string sha) => header.Add(new Sha256Directive(sha));

    public static CspHeader AddScriptSha256(this CspHeader header, Func<string> sha) => header.Add(new Sha256Directive(sha));

    public static CspHeader AllowChildSources(this CspHeader header, params string[] sources) => header.Add(new ChildSourceDirective(sources));

    public static CspHeader AllowConnectSources(this CspHeader header, params string[] sources) => header.Add(new ConnectSourceDirective(sources));

    public static CspHeader AllowFontSources(this CspHeader header, params string[] sources) => header.Add(new FontSourceDirective(sources));

    public static CspHeader AllowFormActions(this CspHeader header, params string[] sources) => header.Add(new FormActionDirective(sources));

    public static CspHeader AllowFrameAncestors(this CspHeader header, params string[] sources) => header.Add(new FrameAncestorsDirective(sources));

    public static CspHeader AllowImageSources(this CspHeader header, params string[] sources) => header.Add(new ImageSourceDirective(sources));

    public static CspHeader AllowManifestSources(this CspHeader header, params string[] sources) => header.Add(new ManifestSourceDirective(sources));

    public static CspHeader AllowMediaSources(this CspHeader header, params string[] sources) => header.Add(new MediaSourceDirective(sources));

    public static CspHeader AllowObjectSources(this CspHeader header, params string[] sources) => header.Add(new ObjectSourceDirective(sources));

    public static CspHeader AllowScriptSources(this CspHeader header, params string[] sources) => header.Add(new ScriptSourceDirective(sources));

    public static CspHeader AllowStyleSources(this CspHeader header, params string[] sources) => header.Add(new StyleSourceDirective(sources));

    public static CspHeader AllowUnsafeEvalScripts(this CspHeader header) => header.Add(new ScriptSourceDirective(CspConstants.UnsafeEvalKeyword));

    public static CspHeader AllowUnsafeInlineScripts(this CspHeader header) => header.Add(new ScriptSourceDirective(CspConstants.UnsafeInlineKeyword));

    public static CspHeader AllowUnsafeInlineStyles(this CspHeader header) => header.Add(new StyleSourceDirective(CspConstants.UnsafeInlineKeyword));

    public static CspHeader ClearDirectives(this CspHeader header) => header.RemoveAll();

    public static CspHeader SetBaseUris(this CspHeader header, params string[] uris) => header.Add(new BaseUriDirective(uris));
    public static CspHeader SetStrictDynamic(this CspHeader header) => header.Add(new ScriptSourceDirective(CspConstants.StrictDynamicKeyword));

    public static CspHeader UpgradeInsecureRequests(this CspHeader header) => header.Add(new UpgradeInsecureRequestsDirective());
}