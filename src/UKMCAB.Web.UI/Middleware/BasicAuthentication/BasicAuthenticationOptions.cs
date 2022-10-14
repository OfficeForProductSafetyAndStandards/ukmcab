using UKMCAB.Common;

namespace UKMCAB.Web.UI.Middleware.BasicAuthentication;

public class BasicAuthenticationOptions
{
    public const string UserName = "internal";
    public bool IsEnabled => Password.Clean() != null;
    public string? Password { get; set; }
}
