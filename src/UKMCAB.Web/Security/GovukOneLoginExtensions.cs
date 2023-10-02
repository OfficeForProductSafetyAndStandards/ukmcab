using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.Users;

namespace UKMCAB.Web.Security;

public static class GovukOneLoginExtensions
{
    public static IServiceCollection AddGovukOneLogin(this IServiceCollection services, IConfiguration configuration)
    {
        var govukOneLogin = new OneLoginHelper(configuration);
        services.AddAuthentication(opt =>
        {
            opt.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie(opt =>
        {
            opt.LoginPath = new PathString("/account/login");
            opt.LogoutPath = new PathString("/account/logout");
            opt.Cookie.Name = "UKMCAB_Identity";
            opt.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        })
        .AddOpenIdConnect(options =>
        {
            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("email");
            options.UsePkce = false;
            options.ResponseMode = OpenIdConnectResponseMode.Query;
            options.ResponseType = OpenIdConnectResponseType.Code;
            options.MetadataAddress = "https://oidc.integration.account.gov.uk/.well-known/openid-configuration";
            options.ClientId = govukOneLogin.ClientId;
            options.CallbackPath = "/oidc";
            options.SaveTokens = true; // important as we may want to use the access_token to call /userinfo endpoint
            options.Events.OnRedirectToIdentityProvider = context =>
            {
                context.ProtocolMessage.SetParameter("vtr", "[\"Cl\"]");
                context.ProtocolMessage.SetParameter("ui_locales", "en");
                return Task.CompletedTask;
            };
            options.Events.OnAuthorizationCodeReceived = context =>
            {
                context.TokenEndpointRequest.ClientAssertionType = OneLoginHelper.ClientAssertionTypeJwtBearer;
                context.TokenEndpointRequest.ClientAssertion = govukOneLogin.CreateClientAuthJwt();
                return Task.CompletedTask;
            };
            options.Events.OnTokenValidated = async context =>
            {
                var identity = context.Principal?.Identity as ClaimsIdentity ?? throw new Exception("Identity did not cast to ClaimsIdentity as expected");
                var accessToken = context.TokenEndpointResponse?.AccessToken ?? throw new Exception("The access token is null");
                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await client.GetAsync("https://oidc.integration.account.gov.uk/userinfo");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var userInfo = JsonSerializer.Deserialize<Dictionary<string, object>>(content);

                var users = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
                var account = await users.GetAsync(context.Principal.FindFirstValue(ClaimTypes.NameIdentifier));
                if(account != null)
                {
                    identity.AddClaim(new Claim(Claims.IsOneLoginUser, string.Empty));
                    SignInHelper.AddClaims(account, identity);
                }
                else
                {
                    identity.AddClaim(new Claim(ClaimTypes.Email, userInfo.GetValueOrDefault("email")?.ToString() ?? string.Empty));
                }
            };

            options.Events.OnRedirectToIdentityProviderForSignOut = async (context) =>
            {
                var uri = context.HttpContext.RequestServices.GetRequiredService<IAppHost>().GetBaseUri();
                context.ProtocolMessage.PostLogoutRedirectUri = uri.ToString(); //"https://find-a-conformity-assessment-body.service.gov.uk/"; //todo: add localhost, dev, stage, preprod and vnext urls to onelogin config
            };
        });
        return services;
    }
}