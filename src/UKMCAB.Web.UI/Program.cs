using Microsoft.Azure.Cosmos;
using UKMCAB.Common.ConnectionStrings;
using UKMCAB.Data.CosmosDb.Services;
using UKMCAB.Infrastructure.Logging;
using UKMCAB.Web.CSP;
using UKMCAB.Web.Middleware;
using UKMCAB.Web.Middleware.BasicAuthentication;
using UKMCAB.Web.UI.Services;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(x => x.AddServerHeader = false);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddAntiforgery();
builder.Services.AddSingleton(new BasicAuthenticationOptions() { Password = builder.Configuration["BasicAuthPassword"] });
builder.Services.AddTransient<ISearchFilterService, SearchFilterService>();
builder.Services.AddTransient<ICABSearchService, CABSearchService>();
builder.Services.AddSingleton(new AzureDataConnectionString(builder.Configuration["DataConnectionString"]));
builder.Services.AddSingleton<ILoggingService, LoggingService>();
builder.Services.AddSingleton<ILoggingRepository, LoggingAzureTableStorageRepository>();
builder.Services.AddCustomHttpErrorHandling();

var cosmosDbSettings = builder.Configuration.GetSection("CosmosDb");
var cosmosConnectionString = builder.Configuration.GetValue<string>("CosmosConnectionString");
if (!string.IsNullOrWhiteSpace(cosmosConnectionString))
{
    var database = cosmosDbSettings.GetValue<string>("Database") ?? "main";
    var container = cosmosDbSettings.GetValue<string>("Container") ?? "cabs";
    builder.Services.AddSingleton<ICosmosDbService>(new CosmosDbService(new CosmosClient(cosmosConnectionString), database, container));
}





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
    //todo: production error pages
    app.UseHsts();
}



app.UseMiddleware<BasicAuthenticationMiddleware>();
app.UseCustomHttpErrorHandling(builder.Environment);
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

app.MapDefaultControllerRoute();



app.Run();
