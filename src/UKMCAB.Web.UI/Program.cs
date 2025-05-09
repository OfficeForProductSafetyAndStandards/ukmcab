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
using UKMCAB.Core.EmailTemplateOptions;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Core.Services.Workflow;
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
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;
using UKMCAB.Web.UI.Models.ViewModels.Search;
using UKMCAB.Web.UI.Services;
using UKMCAB.Web.UI.Services.Subscriptions;
using UKMCAB.Core.Mappers;
using System.Reflection;
using UKMCAB.Web.UI.Services.ReviewDateReminder;
using UKMCAB.Core.Security.Requirements;
using Microsoft.AspNetCore.Authorization;
using UKMCAB.Web.UI.Models.Builders;
using UKMCAB.Data.Interfaces.Services.CAB;
using UKMCAB.Data.Interfaces.Services.CachedCAB;
using UKMCAB.Data.Interfaces.Services.User;
using UKMCAB.Data.Interfaces.Services.WorkflowTask;
using UKMCAB.Data.Interfaces.Services;
using UKMCAB.Data.Caching.CachedCAB;
using System.Text.Json;
using UKMCAB.Data.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using UKMCAB.Data.PostgreSQL.Services;
using UKMCAB.Data.PostgreSQL.Services.WorkflowTask;
using UKMCAB.Data.PostgreSQL.Services.User;
using UKMCAB.Data.PostgreSQL.Services.CAB;
using UKMCAB.Subscriptions.Core.Common;

var builder = WebApplication.CreateBuilder(args);

if (builder.Configuration["AppInsightsConnectionString"].IsNotNullOrEmpty())
{
    builder.Services.AddApplicationInsightsTelemetry(x =>
    {
        x.ConnectionString = builder.Configuration["AppInsightsConnectionString"];
        x.DeveloperMode = builder.Environment.IsDevelopment();
    });
}

//var dataConnectionString = new DataConnectionString(builder.Configuration.GetValue<string>("DataConnectionString"));
var dbConfigJson = builder.Configuration.GetSection("RDS_POSTGRES_CREDENTIALS").Get<string>();
if (dbConfigJson is null)
    throw new InvalidOperationException($"Cannot load connection string configuration");
var dbConfig = JsonSerializer.Deserialize<PostgreDbConfiguration>(dbConfigJson);
var dataConnectionString = $"Server={dbConfig.Host};Port={dbConfig.Port};Database={dbConfig.DbName};User Id={dbConfig.Username};Password={dbConfig.Password};Include Error Detail=true";
builder.Configuration["ConnectionStrings:DefaultConnection"] = dataConnectionString;


var dbSubscriptionsConfigJson = builder.Configuration.GetSection("RDS_POSTGRES_SUBSCRIPTIONS_CREDENTIALS").Get<string>();
if (dbSubscriptionsConfigJson is null)
    throw new InvalidOperationException($"Cannot load subscriptions connection string configuration");
var dbSubscriptionsConfig = JsonSerializer.Deserialize<PostgreDbConfiguration>(dbSubscriptionsConfigJson);
var dataSubscriptionsConnectionString = $"Server={dbSubscriptionsConfig.Host};Port={dbSubscriptionsConfig.Port};Database={dbSubscriptionsConfig.DbName};User Id={dbSubscriptionsConfig.Username};Password={dbSubscriptionsConfig.Password};Include Error Detail=true";

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
    options.AddPolicy(Policies.LegislativeAreaApprove, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(Claims.LegislativeAreaApprove);
    });
    options.AddPolicy(Policies.LegislativeAreaManage, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(Claims.LegislativeAreaManage);
        policy.Requirements.Add(new ManageLegislativeAreaRequirement());
    });
    options.AddPolicy(Policies.EditCabPendingApproval, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new EditCabPendingApprovalRequirement());
    }); 
    options.AddPolicy(Policies.CanRequest, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(Claims.CanRequest);
    });
    options.AddPolicy(Policies.DeleteCab, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new DeleteCabRequirement());
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
builder.Services.AddSingleton(new DataConnectionString(dataConnectionString));
builder.Services.AddSingleton(new SubscriptionDataConnectionString(dataSubscriptionsConnectionString));
builder.Services.AddSingleton<IAsyncNotificationClient>(new NotificationClient(builder.Configuration["GovUkNotifyApiKey"]));

builder.Services.AddDbContextPool<ApplicationDataContext>(options =>
    options.UseNpgsql(dataConnectionString, (NpgsqlDbContextOptionsBuilder sqlOptions) =>
    {
        sqlOptions.MigrationsAssembly(typeof(ApplicationDataContext).Assembly.FullName);

        sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorCodesToAdd: null);
    })
);
builder.Services.AddTransient(typeof(IReadOnlyRepository<>), typeof(PostgreReadOnlyRepository<>));
builder.Services.AddTransient(typeof(IWriteableRepository<>), typeof(PostgreWritableRepository<>));

builder.Services.AddSingleton<IDistCache, RedisCache>();
builder.Services.AddTransient<ICABRepository, PostgreCABRepository>(); 
builder.Services.AddTransient<ICachedPublishedCABService, CachedPublishedCABService>();
builder.Services.AddTransient<ILoggingService, LoggingService>();
builder.Services.AddTransient<ILoggingRepository, LoggingAzureTableStorageRepository>();
builder.Services.AddTransient<IFileStorage, AwsFileStorageService>();
builder.Services.AddTransient<UKMCAB.Data.IInitialiseDataService, UKMCAB.Data.InitialiseDataService>();
builder.Services.AddTransient<IUserAccountRepository, PostgreUserAccountRepository>();
builder.Services.AddTransient<IUserAccountRequestRepository, PostgreUserAccountRequestRepository>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IWorkflowTaskRepository, PostgreWorkflowTaskRepository>();
builder.Services.AddTransient<IWorkflowTaskService, WorkflowTaskService>();
builder.Services.AddTransient<IAppHost, AppHost>();
builder.Services.AddSingleton<ISecureTokenProcessor>(new SecureTokenProcessor(builder.Configuration["EncryptionKey"] ?? throw new Exception("EncryptionKey is null")));
builder.Services.AddTransient<IEditLockService, EditLockService>();
builder.Services.AddTransient<IAuthorizationHandler, EditCabPendingApprovalHandler>();
builder.Services.AddTransient<IAuthorizationHandler, DeleteCabHandler>();
builder.Services.AddTransient<IAuthorizationHandler, ManageLegislativeAreaRequirementHandler>();

builder.Services.AddScoped<ICABAdminService, CABAdminService>();
builder.Services.AddTransient<IUserNoteService, UserNoteService>();
builder.Services.AddTransient<IFeedService, FeedService>();
builder.Services.AddTransient<IFileUploadUtils, FileUploadUtils>();
builder.Services.AddTransient<ILegislativeAreaService, LegislativeAreaService>();
builder.Services.AddTransient<ILegislativeAreaDetailService, LegislativeAreaDetailService>();
builder.Services.AddTransient<ICabSummaryUiService, CabSummaryUiService>();

builder.Services.AddTransient<ICabSummaryViewModelBuilder, CabSummaryViewModelBuilder>();
builder.Services.AddTransient<ICabLegislativeAreasViewModelBuilder, CabLegislativeAreasViewModelBuilder>();
builder.Services.AddTransient<ICabLegislativeAreasItemViewModelBuilder, CabLegislativeAreasItemViewModelBuilder>();

builder.Services.AddCustomHttpErrorHandling();
builder.Services.AddGovUkFrontend();

AddSubscriptionCoreServices(builder, dataSubscriptionsConnectionString);

//builder.Services.AddDataProtection().ProtectKeysWithCertificate(new X509Certificate2(Convert.FromBase64String(builder.Configuration["DataProtectionX509CertBase64"])))
//    .PersistKeysToAzureBlobStorage(dataConnectionString, Constants.Config.ContainerNameDataProtectionKeys, "keys.xml")
//    .SetApplicationName("UKMCAB")
//    .SetDefaultKeyLifetime(TimeSpan.FromDays(365 * 2));

builder.Services.Configure<CoreEmailTemplateOptions>(builder.Configuration.GetSection("GovUkNotifyTemplateOptions"));
builder.Services.AddHostedService<RandomSortGenerator>();
builder.Services.AddSearchService(cognitiveSearchConnectionString);
builder.Services.Configure<CookieTempDataProviderOptions>(options => options.Cookie.Name = "UKMCAB_TempData");
builder.Services.Configure<AntiforgeryOptions>(options => options.Cookie.Name = "UKMCAB_AntiForgery");

builder.Services.AddHttpContextAccessor();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddScoped<IValidator<CABDetailsViewModel>, CABDetailsViewModelValidator>();
builder.Services.AddScoped<IValidator<DeleteCABViewModel>, DeleteCABViewModelValidator>();
builder.Services.AddScoped<IValidator<CABLegislativeAreasViewModel>, CABLegislativeAreasViewModelValidator>();
builder.Services.AddHostedService<ReviewDateReminderBackgroundService>();
builder.Services.AddHostedService<StatsRecorderBackgroundService>();

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

// TODO:
try
{
    using (var scope = app.Services.CreateScope())
    {
        await scope.ServiceProvider.GetRequiredService<UKMCAB.Data.IInitialiseDataService>().InitialiseAsync();
    }
}
catch (Exception ex)
{
    telemetryClient.TrackException(ex);
    await telemetryClient.FlushAsync(CancellationToken.None);
    throw;
}


PostgreAutoMigration.MigrateDatabase(app);
SubscriptionAutoMigration.MigrateDatabase(app);

app.Run();

#region Subscriptions stuff

static void AddSubscriptionCoreServices(WebApplicationBuilder builder, DataConnectionString subscriptionsConnectionString)
{
    var subscriptionsDateTimeProvider = new SubscriptionsDateTimeProvider();
    builder.Services.AddSingleton<IDateTimeProvider>(subscriptionsDateTimeProvider);
    builder.Services.AddSingleton<ISubscriptionsDateTimeProvider>(subscriptionsDateTimeProvider);
    builder.Services.AddTransient<ICabService, SubscriptionsCabService>();          // INJECT OUR OWN `ICabService` as this is slightly more efficient in that it doesn't use a json api
    var subscriptionServicesCoreOptions = new SubscriptionsCoreServicesOptions
    {
        AwsServiceUrl = builder.Configuration.GetSection("Aws:ServiceUrl").Value,
        AwsAccessKey = builder.Configuration.GetSection("Aws:AccessKey").Value ?? string.Empty,
        AwsSecretKey = builder.Configuration.GetSection("Aws:SecretKey").Value ?? string.Empty,
        DataConnectionString = subscriptionsConnectionString,
        SearchQueryStringRemoveKeys = SearchViewModel.NonFilterProperties,
        OutboundEmailSenderMode = OutboundEmailSenderMode.Send,
        GovUkNotifyApiKey = builder.Configuration["GovUkNotifyApiKey"],
        EncryptionKey = builder.Configuration["EncryptionKey"]
            ?? throw new Exception("Configuration item 'EncryptionKey' is not set; here's a key you can use (add to secrets): "
            + UKMCAB.Subscriptions.Core.Common.Security.Tokens.KeyIV.GenerateKey())
    };

    builder.Configuration.Bind("SubscriptionsCoreEmailTemplates", subscriptionServicesCoreOptions.EmailTemplates);

    builder.Services.AddSubscriptionsCoreServices(subscriptionServicesCoreOptions);

    builder.Services.AddTransient<ISubscriptionEngineCoordinator, SubscriptionEngineCoordinator>();

    builder.Services.AddTransient<SubscriptionsBackgroundService>();
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
