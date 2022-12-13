using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Cosmos;
using System.Security.Cryptography.X509Certificates;
using Notify.Client;
using Notify.Interfaces;
using UKMCAB.Common.ConnectionStrings;
using UKMCAB.Core.Services.Account;
using UKMCAB.Data.CosmosDb.Services;
using UKMCAB.Identity.Stores.CosmosDB;
using UKMCAB.Identity.Stores.CosmosDB.Extensions;
using UKMCAB.Identity.Stores.CosmosDB.Stores;
using UKMCAB.Infrastructure.Cache;
using UKMCAB.Infrastructure.Logging;
using UKMCAB.Web.CSP;
using UKMCAB.Web.Middleware;
using UKMCAB.Web.Middleware.BasicAuthentication;
using UKMCAB.Web.UI;
using UKMCAB.Web.UI.Services;

var builder = WebApplication.CreateBuilder(args);

var azureDataConnectionString = new AzureDataConnectionString(builder.Configuration["DataConnectionString"]);

builder.WebHost.ConfigureKestrel(x => x.AddServerHeader = false);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddAntiforgery();
builder.Services.AddSingleton(new BasicAuthenticationOptions() { Password = builder.Configuration["BasicAuthPassword"] });
builder.Services.AddSingleton(new RedisConnectionString(builder.Configuration["RedisConnectionString"]));
builder.Services.AddTransient<ISearchFilterService, SearchFilterService>();
builder.Services.AddTransient<ICABSearchService, CABSearchService>();
builder.Services.AddTransient<IAdminService, AdminService>();
builder.Services.AddSingleton(azureDataConnectionString);
builder.Services.AddSingleton<ILoggingService, LoggingService>();
builder.Services.AddSingleton<ILoggingRepository, LoggingAzureTableStorageRepository>();
builder.Services.AddSingleton<IDistCache, RedisCache>();
builder.Services.AddSingleton<IAsyncNotificationClient>(new NotificationClient(builder.Configuration["GovUkNotifyApiKey"]));
builder.Services.AddSingleton<IRegisterService, RegisterService>();
builder.Services.AddCustomHttpErrorHandling();

builder.Services.AddDataProtection().ProtectKeysWithCertificate(new X509Certificate2(Convert.FromBase64String(builder.Configuration["DataProtectionX509CertBase64"])))
    .PersistKeysToAzureBlobStorage(azureDataConnectionString, Constants.Config.ContainerNameDataProtectionKeys, "keys.xml")
    .SetApplicationName("UKMCAB")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(365 * 2));

builder.Services.Configure<TemplateOptions>(builder.Configuration.GetSection("GovUkNotifyTemplateIds"));

var cosmosDbSettings = builder.Configuration.GetSection("CosmosDb");
var cosmosConnectionString = builder.Configuration.GetValue<string>("CosmosConnectionString");
if (!string.IsNullOrWhiteSpace(cosmosConnectionString))
{
    var database = cosmosDbSettings.GetValue<string>("Database") ?? "main";
    var container = cosmosDbSettings.GetValue<string>("Container") ?? "cabs";
    builder.Services.AddSingleton<ICosmosDbService>(new CosmosDbService(new CosmosClient(cosmosConnectionString), database, container));
}

builder.Services.Configure<IdentityStoresOptions>(options =>
    options.UseAzureCosmosDB(cosmosConnectionString, databaseId: "UKMCABIdentity", containerId: "AppIdentity"));

builder.Services.AddDefaultIdentity<UKMCABUser>(options =>
    {
        options.Password.RequiredLength = 8;
    })
    .AddRoles<IdentityRole>()
    //.AddUserManager<UserManager<UKMCABUser>>()
    .AddAzureCosmosDbStores();
builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = new PathString("/account/login");
    opt.LogoutPath = new PathString("/account/logout");
});

// =================================================================================================

var app = builder.Build();

app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("Referrer-Policy", "no-referrer");
    context.Response.Headers.Add("Permissions-Policy", "accelerometer=(), ambient-light-sensor=(), autoplay=(), battery=(), camera=(), cross-origin-isolated=(), display-capture=(), document-domain=(), encrypted-media=(), execution-while-not-rendered=(), execution-while-out-of-viewport=(), fullscreen=(), geolocation=(), gyroscope=(), keyboard-map=(), magnetometer=(), microphone=(), midi=(), navigation-override=(), payment=(), picture-in-picture=(), publickey-credentials-get=(), screen-wake-lock=(), sync-xhr=(), usb=(), web-share=(), xr-spatial-tracking=(), clipboard-read=(), clipboard-write=(), gamepad=(), speaker-selection=(), conversion-measurement=(), focus-without-user-activation=(), hid=(), idle-detection=(), interest-cohort=(), serial=(), sync-script=(), trust-token-redemption=(), unload=(), window-placement=(), vertical-scroll=()");
    await next();
});

app.MapRazorPages();



var cspHeader = new CspHeader().AddDefaultCspDirectives()
    .AddScriptNonce("VQ8uRGcAff")
    .AddScriptNonce("uKK1n1fxoi")
    .AllowFontSources(CspConstants.SelfKeyword, "https://cdnjs.cloudflare.com")
    .AllowScriptSources("https://cdnjs.cloudflare.com")
    .AllowStyleSources("https://cdnjs.cloudflare.com");

/*
 * 
 * IF STOPGAP ADMIN FORM IS STILL AROUND, WE NEED A MORE LAX CSP
 * 
 */
cspHeader.AllowUnsafeInlineScripts();
cspHeader.AllowUnsafeInlineStyles();
cspHeader.AllowUnsafeEvalScripts();
/*
 * END
 */

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    cspHeader.AllowConnectSources("wss://localhost:*"); // allow hot-reload WSS in development only.
}
else
{
    app.UseHsts();
}

app.UseMiddleware<BasicAuthenticationMiddleware>();
app.UseCustomHttpErrorHandling(builder.Configuration);
app.UseRouting();
app.UseCsp(cspHeader); // content-security-policy
app.UseStaticFiles();

var supportedCultures = new[] { new System.Globalization.CultureInfo("en-GB") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("en-GB"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures,
    FallBackToParentCultures = false,
    FallBackToParentUICultures = false,
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "Account",
    pattern: "{area:exists}/{controller=Home}/{action=Login}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//app.MapDefaultControllerRoute();
app.MapRazorPages();

await app.InitialiseIdentitySeedingAsync<UKMCABUser, IdentityRole>(azureDataConnectionString, Constants.Config.ContainerNameDataProtectionKeys, seeds =>
{
    var opssAdmin = new IdentityRole(Constants.Roles.OPSSAdmin);
    var ogdUser = new IdentityRole(Constants.Roles.OGDUser);
    var ukasUser = new IdentityRole(Constants.Roles.UKASUser);
    seeds
        .AddRole(role: opssAdmin)
        .AddRole(role: ogdUser)
        .AddRole(role: ukasUser)
        .AddUser(user: new() { Email = "admin@ukmcab.gov.uk", UserName = "admin@ukmcab.gov.uk", EmailConfirmed = true, Regulations = new List<string>{"Construction"}, RequestReason = "Seeded", RequestApproved = true},
            password: "adminP@ssw0rd!", roles: opssAdmin)
        .AddUser(user: new() { Email = "ogduser@ukmcab.gov.uk", UserName = "ogduser@ukmcab.gov.uk", EmailConfirmed = true, Regulations = new List<string> { "Construction" }, RequestReason = "Seeded", RequestApproved = true },
            password: "ogdP@ssw0rd!", roles: ogdUser)
        .AddUser(user: new() { Email = "ukasuser@ukas.com", UserName = "ukasuser@ukas.com", EmailConfirmed = true, Regulations = new List<string>(), RequestReason = "Seeded", RequestApproved = true },
            password: "ukasP@ssw0rd!", roles: ukasUser);

    // Note: Username should be provided as its a required field in identity framework and email should be marked as confirmed to allow login, also password should meet identity password requirements
});

await app.Services.GetRequiredService<IDistCache>().InitialiseAsync();




app.Run();

