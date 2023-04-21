using UKMCAB.Subscriptions.Core.Domain.Emails;
using UKMCAB.Web.Middleware.ExceptionHandling;
using UKMCAB.Web.Middleware;

namespace UKMCAB.Web.UI.Services.Subscriptions;

public class SubscriptionsUriTemplatesConfiguratorMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IEmailTemplatesService _emailTemplatesService;
    private readonly LinkGenerator _linkGenerator;

    public SubscriptionsUriTemplatesConfiguratorMiddleware(RequestDelegate next, IEmailTemplatesService emailTemplatesService, LinkGenerator linkGenerator)
    {
        _next = next;
        _emailTemplatesService = emailTemplatesService;
        _linkGenerator = linkGenerator;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_emailTemplatesService.IsConfigured())
        {
            _emailTemplatesService.Configure(new UKMCAB.Subscriptions.Core.Domain.Emails.Uris.UriTemplateOptions
            {
                BaseUri = context.Request.GetRequestUri(),
                CabDetails = new UKMCAB.Subscriptions.Core.Domain.Emails.Uris.ViewCabUriTemplateOptions("@cabid", 
                    _linkGenerator.GetUriByRouteValues(context, Areas.Search.Controllers.CABController.Routes.CabDetails, new { id = "@cabid" }) 
                    ?? throw new Exception("Cab details route not found")),
            });
        }

        await _next(context);
    }
}

public static class SubscriptionsUriTemplatesConfiguratorMiddlewareExtensions
{
    public static IApplicationBuilder UseSubscriptionsUriTemplatesConfiguratorMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<SubscriptionsUriTemplatesConfiguratorMiddleware>();
        return app;
    }
}