using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Cosmos.Linq;
using Notify.Client;
using Notify.Interfaces;
using System.Security.Cryptography.X509Certificates;
using UKMCAB.Common.ConnectionStrings;
using UKMCAB.Core.Services;
using UKMCAB.Data;
using UKMCAB.Data.CosmosDb.Services;
using UKMCAB.Data.Search.Services;
using UKMCAB.Data.Storage;
using UKMCAB.Identity.Stores.CosmosDB;
using UKMCAB.Identity.Stores.CosmosDB.Extensions;
using UKMCAB.Identity.Stores.CosmosDB.Stores;
using UKMCAB.Infrastructure.Cache;
using UKMCAB.Infrastructure.Logging;
using UKMCAB.Subscriptions.Core;
using UKMCAB.Subscriptions.Core.Data;
using UKMCAB.Subscriptions.Core.Domain;
using UKMCAB.Subscriptions.Core.Integration.CabService;
using UKMCAB.Subscriptions.Core.Integration.OutboundEmail;
using UKMCAB.Web.CSP;
using UKMCAB.Web.Middleware;
using UKMCAB.Web.Middleware.BasicAuthentication;
using UKMCAB.Web.UI;
using UKMCAB.Web.UI.Models.ViewModels.Search;
using UKMCAB.Web.UI.Services;
using UKMCAB.Web.UI.Services.Subscriptions;

var builder = WebApplication.CreateBuilder(args);

if (builder.Configuration["AppInsightsConnectionString"].IsNotNullOrEmpty())
{
    builder.Services.AddApplicationInsightsTelemetry(x =>
    {
        x.ConnectionString = builder.Configuration["AppInsightsConnectionString"];
        x.DeveloperMode = builder.Environment.IsDevelopment();
    });
}

var azureDataConnectionString = new AzureDataConnectionString(builder.Configuration["DataConnectionString"]);
var cosmosDbConnectionString = new CosmosDbConnectionString(builder.Configuration.GetValue<string>("CosmosConnectionString"));
var cognitiveSearchConnectionString = new CognitiveSearchConnectionString(builder.Configuration["AcsConnectionString"]);

var redisConnectionString = builder.Configuration["RedisConnectionString"];
if (!redisConnectionString.Contains("allowAdmin"))
{
    redisConnectionString = redisConnectionString + ",allowAdmin=true";
}

builder.WebHost.ConfigureKestrel(x => x.AddServerHeader = false);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddAntiforgery(x => x.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest);
builder.Services.AddHsts(x => x.MaxAge = TimeSpan.FromDays(370));
builder.Services.AddSingleton(new BasicAuthenticationOptions 
{ 
    Password = builder.Configuration["BasicAuthPassword"], 
    ExclusionPaths = new() { "/search-feed", "/search/cab-profile-feed" }
});
builder.Services.AddSingleton(new RedisConnectionString(redisConnectionString));
builder.Services.AddSingleton(cognitiveSearchConnectionString);
builder.Services.AddSingleton(cosmosDbConnectionString);
builder.Services.AddSingleton(azureDataConnectionString);
builder.Services.AddSingleton<IAsyncNotificationClient>(new NotificationClient(builder.Configuration["GovUkNotifyApiKey"]));

builder.Services.AddTransient<IAdminService, AdminService>();
builder.Services.AddSingleton<IDistCache, RedisCache>();
builder.Services.AddSingleton<ICABRepository, CABRepository>(); 
builder.Services.AddTransient<ICABAdminService, CABAdminService>();
builder.Services.AddSingleton<ICachedPublishedCABService, CachedPublishedCABService>();
builder.Services.AddTransient<IFeedService, FeedService>();
builder.Services.AddSingleton<ILoggingService, LoggingService>();
builder.Services.AddSingleton<ILoggingRepository, LoggingAzureTableStorageRepository>();
builder.Services.AddSingleton<IFileStorage, FileStorageService>();
builder.Services.AddSingleton<IInitialiseDataService, InitialiseDataService>();
builder.Services.AddCustomHttpErrorHandling();

AddSubscriptionCoreServices(builder, azureDataConnectionString);

builder.Services.AddDataProtection().ProtectKeysWithCertificate(new X509Certificate2(Convert.FromBase64String(builder.Configuration["DataProtectionX509CertBase64"])))
    .PersistKeysToAzureBlobStorage(azureDataConnectionString, Constants.Config.ContainerNameDataProtectionKeys, "keys.xml")
    .SetApplicationName("UKMCAB")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(365 * 2));

builder.Services.Configure<TemplateOptions>(builder.Configuration.GetSection("GovUkNotifyTemplateOptions"));


builder.Services.AddHostedService<RandomSortGenerator>();


builder.Services.AddSearchService(cognitiveSearchConnectionString);

builder.Services.Configure<IdentityStoresOptions>(options =>
    options.UseAzureCosmosDB(cosmosDbConnectionString, databaseId: "UKMCABIdentity", containerId: "AppIdentity"));

builder.Services.AddDefaultIdentity<UKMCABUser>(options =>
{
    options.Password.RequiredLength = 8;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromHours(2);
})
    .AddRoles<IdentityRole>()
    .AddAzureCosmosDbStores();

builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = new PathString("/account/login");
    opt.LogoutPath = new PathString("/account/logout");
    opt.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    opt.Cookie.Name = "UKMCAB_Identity";
});

builder.Services.Configure<CookieTempDataProviderOptions>(options => options.Cookie.Name = "UKMCAB_TempData");
builder.Services.Configure<AntiforgeryOptions>(options => options.Cookie.Name = "UKMCAB_AntiForgery");



// =================================================================================================



var app = builder.Build();

app.UseCookiePolicy(new CookiePolicyOptions { Secure = CookieSecurePolicy.SameAsRequest });

app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-XSS-Protection", "0"); // deprecated header, recommendation is to turn off with '0' value, in favour of a strong CSP header.
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("Referrer-Policy", "no-referrer");
    context.Response.Headers.Add("Permissions-Policy", "accelerometer=(), ambient-light-sensor=(), autoplay=(), battery=(), camera=(), cross-origin-isolated=(), display-capture=(), document-domain=(), encrypted-media=(), execution-while-not-rendered=(), execution-while-out-of-viewport=(), fullscreen=(), geolocation=(), gyroscope=(), keyboard-map=(), magnetometer=(), microphone=(), midi=(), navigation-override=(), payment=(), picture-in-picture=(), publickey-credentials-get=(), screen-wake-lock=(), sync-xhr=(), usb=(), web-share=(), xr-spatial-tracking=(), clipboard-read=(), clipboard-write=(), gamepad=(), speaker-selection=(), conversion-measurement=(), focus-without-user-activation=(), hid=(), idle-detection=(), interest-cohort=(), serial=(), sync-script=(), trust-token-redemption=(), unload=(), window-placement=(), vertical-scroll=()");
    await next();
});

app.MapRazorPages();

var cspHeader = new CspHeader().AddDefaultCspDirectives()
    .AddScriptNonce(Nonces.Main)
    .AddScriptNonce(Nonces.GoogleAnalyticsScript)
    .AddScriptNonce(Nonces.GoogleAnalyticsInlineScript)
    .AddScriptNonce(Nonces.AppInsights)
    .AllowFontSources(CspConstants.SelfKeyword, "https://cdnjs.cloudflare.com")
    .AllowScriptSources("https://cdnjs.cloudflare.com", "https://js.monitor.azure.com", "https://region1.google-analytics.com")
    .AllowStyleSources("https://cdnjs.cloudflare.com")
    .AllowConnectSources("https://uksouth-1.in.applicationinsights.azure.com", "https://region1.google-analytics.com");

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

app.UseCsp(cspHeader); // content-security-policy
app.UseMiddleware<BasicAuthenticationMiddleware>();
app.UseCustomHttpErrorHandling(builder.Configuration);
app.UseRouting();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = (ctx) =>
    {
        if (ctx.File.Name.ToLower().EndsWith(".css", StringComparison.OrdinalIgnoreCase) || ctx.File.Name.ToLower().EndsWith(".js", StringComparison.OrdinalIgnoreCase))
        {
            ctx.Context.Response.Headers.CacheControl = "no-cache, no-store";
            ctx.Context.Response.Headers.Pragma = "no-cache";
            ctx.Context.Response.Headers.Expires = "-1";
        }
    }
});


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
    pattern: "{controller=Search}/{action=Index}/{id?}");

UseSubscriptions(app);

await app.InitialiseIdentitySeedingAsync<UKMCABUser, IdentityRole>(azureDataConnectionString, Constants.Config.ContainerNameDataProtectionKeys, seeds =>
{
    seeds.AddRole(role: new IdentityRole(Constants.Roles.OPSSAdmin));
});

var logger = app.Services.GetRequiredService<ILogger<Program>>();
var telemetryClient = app.Services.GetRequiredService<TelemetryClient>();

await app.Services.GetRequiredService<IDistCache>().InitialiseAsync();

try
{
    await app.Services.GetRequiredService<IInitialiseDataService>().InitialiseAsync();
}
catch (Exception ex)
{
    telemetryClient.TrackException(ex);
    await telemetryClient.FlushAsync(CancellationToken.None);
    throw;
}

_ = Task.Run(async () => // asynchronously precache
{
    try
    {
        await Task.Delay(5000);
        var count = await app.Services.GetRequiredService<ICachedPublishedCABService>().PreCacheAllCabsAsync();
        logger.LogInformation($"Precached {count} CABs");
    }
    catch (Exception ex)
    {
        telemetryClient.TrackException(ex);
        await telemetryClient.FlushAsync(CancellationToken.None);
        logger.LogError(ex, "Error precaching");
    }
});

var stats = new Timer(async state =>
{
    using var scope = app.Services.CreateScope();
    try
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UKMCABUser>>();
        telemetryClient.TrackMetric(AiTracking.Metrics.UsersCount, await userManager.Users.CountAsync());
        await app.Services.GetRequiredService<ICABAdminService>().RecordStatsAsync();

        var pages1 = await app.Services.GetRequiredService<ISubscriptionRepository>().GetAllAsync(take: 1);
        var pages2 = await pages1.ToListAsync();
        var subscriptions = pages2.SelectMany(x => x.Values).ToList();

        var cabSubscriptionsCount = subscriptions.Count(x => x.SubscriptionType == SubscriptionType.Cab);
        var searchSubscriptionsCount = subscriptions.Count(x => x.SubscriptionType == SubscriptionType.Search);

        telemetryClient.GetMetric(AiTracking.Metrics.CabSubscriptionsCount).TrackValue(cabSubscriptionsCount);
        telemetryClient.GetMetric(AiTracking.Metrics.SearchSubscriptionsCount).TrackValue(searchSubscriptionsCount);

        var cabs = await app.Services.GetRequiredService<ICABRepository>().GetItemLinqQueryable().AsAsyncEnumerable().ToListAsync();
        telemetryClient.GetMetric(AiTracking.Metrics.CabsWithSchedules).TrackValue(cabs.Count(x => (x.Schedules ?? new()).Count > 0));
        telemetryClient.GetMetric(AiTracking.Metrics.CabsWithoutSchedules).TrackValue(cabs.Count(x => (x.Schedules ?? new()).Count == 0));

        logger.LogInformation($"Recorded stats successfully");
    }
    catch (Exception ex)
    {
        telemetryClient.TrackException(ex);
        await telemetryClient.FlushAsync(CancellationToken.None);
        logger.LogError(ex, "Error recording stats");
    }
}, null, TimeSpan.Zero,
#if(DEBUG)
    TimeSpan.FromSeconds(30)
#else
    TimeSpan.FromDays(1)
#endif
);

app.Run();

#region Subscriptions stuff

static void AddSubscriptionCoreServices(WebApplicationBuilder builder, AzureDataConnectionString azureDataConnectionString)
{
    var subscriptionsDateTimeProvider = new SubscriptionsDateTimeProvider();
    builder.Services.AddSingleton<IDateTimeProvider>(subscriptionsDateTimeProvider);
    builder.Services.AddSingleton<ISubscriptionsDateTimeProvider>(subscriptionsDateTimeProvider);
    builder.Services.AddSingleton<ICabService, SubscriptionsCabService>();          // INJECT OUR OWN `ICabService` as this is slightly more efficient in that it doesn't use a json api
    var subscriptionServicesCoreOptions = new SubscriptionsCoreServicesOptions
    {
        DataConnectionString = builder.Configuration["subscriptions_data_connstr"] ?? azureDataConnectionString,
        SearchQueryStringRemoveKeys = SearchViewModel.NonFilterProperties,
        OutboundEmailSenderMode = OutboundEmailSenderMode.Send,
        GovUkNotifyApiKey = builder.Configuration["GovUkNotifyApiKey"],
        EncryptionKey = builder.Configuration["EncryptionKey"] 
            ?? throw new Exception("Configuration item 'EncryptionKey' is not set; here's a key you can use (add to secrets): " 
            + UKMCAB.Subscriptions.Core.Common.Security.Tokens.KeyIV.GenerateKey())
    };

    builder.Configuration.Bind("SubscriptionsCoreEmailTemplates", subscriptionServicesCoreOptions.EmailTemplates);

    builder.Services.AddSubscriptionsCoreServices(subscriptionServicesCoreOptions);

    builder.Services.AddSingleton<ISubscriptionEngineCoordinator, SubscriptionEngineCoordinator>();

    builder.Services.AddSingleton<SubscriptionsBackgroundService>();
    builder.Services.AddHostedService(p => p.GetRequiredService<SubscriptionsBackgroundService>());
    builder.Services.AddHostedService<SubscriptionsConfiguratorHostedService>();
}

static void UseSubscriptions(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseSubscriptionsDiagnostics();
    }
}

#endregion