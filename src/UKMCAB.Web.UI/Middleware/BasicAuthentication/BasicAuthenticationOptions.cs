namespace UKMCAB.Web.UI.Middleware.BasicAuthentication;

public class BasicAuthenticationOptions
{
    public bool IsEnabled { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
}
