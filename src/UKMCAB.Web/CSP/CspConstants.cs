namespace UKMCAB.Web.CSP;

public static class CspConstants
{
    public const string BlobValue = "blob:";
    public const string CspHeader = "Content-Security-Policy";

    public const string DataValue = "data:";
    public const string FilesystemValue = "filesystem:";
    public const string HttpsValue = "https:";
    public const string MediastreamValue = "mediastream:";
    public const string NoneKeyword = "'none'";
    public const string SelfKeyword = "'self'";
    public const string StrictDynamicKeyword = "'strict-dynamic'";
    public const string UnsafeEvalKeyword = "'unsafe-eval'";
    public const string UnsafeInlineKeyword = "'unsafe-inline'";
    public const string WildCardSymbol = "*";

    internal const string BaseUriDirectiveKey = "base-uri";
    internal const string ChildSourceDirectiveKey = "child-src";
    internal const string ConnectSourceDirectiveKey = "connect-src";
    internal const string DefaultSourceDirectiveKey = "default-src";
    internal const string FontSourceDirectiveKey = "font-src";
    internal const string FormActionDirectiveKey = "form-action";
    internal const string FrameAncestorsDirectiveKey = "frame-ancestors";
    internal const string ImageSourceDirectiveKey = "img-src";
    internal const string ManifestSourceDirectiveKey = "manifest-src";
    internal const string MediaSourceDirectiveKey = "media-src";
    internal const string NoncePrefix = "nonce-";
    internal const string ObjectSourceDirectiveKey = "object-src";
    internal const string PluginTypesDirectiveKey = "plugin-types";
    internal const string ReportUriDirectiveKey = "report-uri";
    internal const string SandboxDirectiveKey = "sandbox";
    internal const string ScriptSourceDirectiveKey = "script-src";
    internal const string Sha256Prefix = "sha256-";
    internal const string StyleSourceDirectiveKey = "style-src";
    internal const string UpgradeInsecureRequestsKey = "upgrade-insecure-requests";
}