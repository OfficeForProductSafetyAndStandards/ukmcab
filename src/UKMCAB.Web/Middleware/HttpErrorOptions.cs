namespace UKMCAB.Web.Middleware;
public class HttpErrorOptions
{
    public string Error400Path { get; set; } = "/400";
    public string Error401Path { get; set; } = "/401";
    public string Error403Path { get; set; } = "/403";
    public string Error404Path { get; set; } = "/404";
    public string Error500Path { get; set; } = "/500";
}
