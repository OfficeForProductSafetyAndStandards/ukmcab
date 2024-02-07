using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Features;
using Notify.Client;
using Notify.Interfaces;
using System.Security.Cryptography.X509Certificates;
using GovUk.Frontend.AspNetCore;
using UKMCAB.Common.ConnectionStrings;
using UKMCAB.Common.Security.Tokens;
using UKMCAB.Core;
using UKMCAB.Core.EmailTemplateOptions;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Core.Services.Workflow;
using UKMCAB.Data;
using UKMCAB.Data.CosmosDb.Services.CAB;
using UKMCAB.Data.CosmosDb.Services.CachedCAB;
using UKMCAB.Data.CosmosDb.Services.User;
using UKMCAB.Data.CosmosDb.Services.WorkflowTask;
using UKMCAB.Data.Search.Services;
using UKMCAB.Data.Storage;
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
using UKMCAB.Web.Security;
using UKMCAB.Web.UI;
using UKMCAB.Web.UI.Models.ViewModels.Admin;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;
using UKMCAB.Web.UI.Models.ViewModels.Search;
using UKMCAB.Web.UI.Services;
using UKMCAB.Web.UI.Services.Subscriptions;
using UKMCAB.Data.Models.LegislativeAreas;
using UKMCAB.Data.CosmosDb.Services;
using UKMCAB.Data.CosmosDb;
using UKMCAB.Data.CosmosDb.Utilities;
using UKMCAB.Core.Mappers;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

if (builder.Configuration["AppInsightsConnectionString"].IsNotNullOrEmpty())
{
    builder.Services.AddApplicationInsightsTelemetry(x =>
    {
        x.ConnectionString = builder.Configuration["AppInsightsConnectionString"];
        x.DeveloperMode = builder.Environment.IsDevelopment();
    });
}

var azureDataConnectionString = new AzureDataConnectionString(builder.Configuration.GetValue<string>("DataConnectionString"));
var cosmosDbConnectionString = new CosmosDbConnectionString(builder.Configuration.GetValue<string>("CosmosConnectionString"));
var cognitiveSearchConnectionString = new CognitiveSearchConnectionString(builder.Configuration["AcsConnectionString"]);

var redisConnectionString = builder.Configuration.GetValue<string>("RedisConnectionString");
if (!redisConnectionString.Contains("allowAdmin"))
{
    redisConnectionString += ",allowAdmin=true";
}
builder.Services.Configure<FormOptions>(options => options.MultipartBodyLengthLimit = 367001600);

builder.WebHost.ConfigureKestrel(x =>
{
    x.AddServerHeader = false;
    x.Limits.MaxRequestBodySize = 367001600;
});
builder.Services.AddGovukOneLogin(builder.Configuration);
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.CabManagement, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(Claims.CabManagement);
    });
    options.AddPolicy(Policies.UserManagement, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(Claims.UserManagement);
    });
    options.AddPolicy(Policies.GovernmentUserNotes, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(Claims.CabGovernmentUserNotes);
    });
    options.AddPolicy(Policies.ApproveRequests, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(Claims.CabCanApprove);
    });
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddAntiforgery(x => x.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest);
builder.Services.AddHsts(x => x.MaxAge = TimeSpan.FromDays(370));
builder.Services.AddAutoMapper(typeof(AutoMapperProfile).GetTypeInfo().Assembly);

builder.Services.AddSingleton(new BasicAuthenticationOptions 
{ 
    Password = builder.Configuration["BasicAuthPassword"], 
    ExclusionPaths = new List<string> { "/search-feed", "/search/cab-profile-feed" }
});
builder.Services.AddSingleton(new RedisConnectionString(redisConnectionString));
builder.Services.AddSingleton(cognitiveSearchConnectionString);
builder.Services.AddSingleton(cosmosDbConnectionString);
builder.Services.AddSingleton(azureDataConnectionString);
builder.Services.AddSingleton<IAsyncNotificationClient>(new NotificationClient(builder.Configuration["GovUkNotifyApiKey"]));

builder.Services.AddSingleton<IDistCache, RedisCache>();
builder.Services.AddSingleton<ICABRepository, CABRepository>(); 
builder.Services.AddSingleton<ICachedPublishedCABService, CachedPublishedCABService>();
builder.Services.AddSingleton<ILoggingService, LoggingService>();
builder.Services.AddSingleton<ILoggingRepository, LoggingAzureTableStorageRepository>();
builder.Services.AddSingleton<IFileStorage, FileStorageService>();
builder.Services.AddSingleton<IInitialiseDataService, InitialiseDataService>();
builder.Services.AddSingleton<IUserAccountRepository, UserAccountRepository>();
builder.Services.AddSingleton<IUserAccountRequestRepository, UserAccountRequestRepository>();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<IWorkflowTaskRepository, WorkflowTaskRepository>();
builder.Services.AddSingleton<IWorkflowTaskService, WorkflowTaskService>();
builder.Services.AddSingleton<IAppHost, AppHost>();
builder.Services.AddSingleton<ISecureTokenProcessor>(new SecureTokenProcessor(builder.Configuration["EncryptionKey"] ?? throw new Exception("EncryptionKey is null")));
builder.Services.AddSingleton<IEditLockService, EditLockService>();

var cosmosClient = CosmosClientFactory.Create(cosmosDbConnectionString);
builder.Services.AddSingleton<IReadOnlyRepository<LegislativeArea>>(new ReadOnlyRepository<LegislativeArea>(cosmosClient, new CosmosFeedIterator(), "legislative-areas"));
builder.Services.AddSingleton<IReadOnlyRepository<PurposeOfAppointment>>(new ReadOnlyRepository<PurposeOfAppointment>(cosmosClient, new CosmosFeedIterator(), "purpose-of-appointment"));
builder.Services.AddSingleton<IReadOnlyRepository<Category>>(new ReadOnlyRepository<Category>(cosmosClient, new CosmosFeedIterator(), "categories"));
builder.Services.AddSingleton<IReadOnlyRepository<Product>>(new ReadOnlyRepository<Product>(cosmosClient, new CosmosFeedIterator(), "products"));
builder.Services.AddSingleton<IReadOnlyRepository<Procedure>>(new ReadOnlyRepository<Procedure>(cosmosClient, new CosmosFeedIterator(), "procedures"));

builder.Services.AddTransient<ICABAdminService, CABAdminService>();
builder.Services.AddTransient<IUserNoteService, UserNoteService>();
builder.Services.AddTransient<IFeedService, FeedService>();
builder.Services.AddTransient<IFileUploadUtils, FileUploadUtils>();
builder.Services.AddTransient<ILegislativeAreaService, LegislativeAreaService>();

builder.Services.AddCustomHttpErrorHandling();
builder.Services.AddGovUkFrontend();

AddSubscriptionCoreServices(builder, azureDataConnectionString);

builder.Services.AddDataProtection().ProtectKeysWithCertificate(new X509Certificate2(Convert.FromBase64String(builder.Configuration["DataProtectionX509CertBase64"])))
    .PersistKeysToAzureBlobStorage(azureDataConnectionString, Constants.Config.ContainerNameDataProtectionKeys, "keys.xml")
    .SetApplicationName("UKMCAB")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(365 * 2));

builder.Services.Configure<CoreEmailTemplateOptions>(builder.Configuration.GetSection("GovUkNotifyTemplateOptions"));
builder.Services.AddHostedService<RandomSortGenerator>();
builder.Services.AddSearchService(cognitiveSearchConnectionString);
builder.Services.Configure<CookieTempDataProviderOptions>(options => options.Cookie.Name = "UKMCAB_TempData");
builder.Services.Configure<AntiforgeryOptions>(options => options.Cookie.Name = "UKMCAB_AntiForgery");

builder.Services.AddHttpContextAccessor();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddScoped<IValidator<CABDetailsViewModel>, CABDetailsViewModelValidator>();
builder.Services.AddScoped<IValidator<DeleteCABViewModel>, DeleteCABViewModelValidator>();

// =================================================================================================

var app = builder.Build();

app.UseCookiePolicy(new CookiePolicyOptions { Secure = CookieSecurePolicy.SameAsRequest });

app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-XSS-Protection", "0"); // deprecated header, recommendation is to turn off with '0' value, in favour of a strong CSP header.
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("Referrer-Policy", "no-referrer");
    context.Response.Headers.Add("Permissions-Policy", "accelerometer=(), ambient-light-sensor=(), autoplay=(), battery=(), camera=(), cross-origin-isolated=(), display-capture=(), document-domain=(), encrypted-media=(), execution-while-not-rendered=(), execution-while-out-of-viewport=(), fullscreen=(), geolocation=(), gyroscope=(), keyboard-map=(), magnetometer=(), microphone=(), midi=(), navigation-override=(), payment=(), picture-in-picture=(), publickey-credentials-get=(), screen-wake-lock=(), sync-xhr=(), usb=(), web-share=(), xr-spatial-tracking=(), clipboard-read=(self), clipboard-write=(self), gamepad=(), speaker-selection=(), conversion-measurement=(), focus-without-user-activation=(), hid=(), idle-detection=(), interest-cohort=(), serial=(), sync-script=(), trust-token-redemption=(), unload=(), window-placement=(), vertical-scroll=()");
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
cspHeader.AllowImageSources("data:");
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

app.UseMiddleware<AccountStatusCheckMiddleware>();

app.MapControllerRoute(
    name: "Account",
    pattern: "{area:exists}/{controller=Home}/{action=Login}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Search}/{action=Index}/{id?}");

UseSubscriptions(app);

var logger = app.Services.GetRequiredService<ILogger<Program>>();
var telemetryClient = app.Services.GetRequiredService<TelemetryClient>();
var redisCache = app.Services.GetRequiredService<IDistCache>();
await redisCache.InitialiseAsync();
await redisCache.FlushAsync();

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

var stats = new Timer(async state =>
{
    using var scope = app.Services.CreateScope();
    try
    {
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




