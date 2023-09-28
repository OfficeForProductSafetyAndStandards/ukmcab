using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using UKMCAB.Core.Services.Users;

public class AccountStatusCheckMiddleware
{
    private readonly RequestDelegate _next;

    public AccountStatusCheckMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.User?.Identity?.IsAuthenticated ?? false)
        {
            var isActive = await IsUserAccountActiveAsync(context);
            if (!isActive)
            {
                context.Response.Redirect("/account/login");
            }
        }
        await _next(context);
    }

    private static async Task<bool> IsUserAccountActiveAsync(HttpContext context)
    {
        var users = context.RequestServices.GetRequiredService<IUserService>();
        var id = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isActive = await users.IsActiveAsync(id);
        return isActive;
    }
}
