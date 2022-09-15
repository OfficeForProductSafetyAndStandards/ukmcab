using UKMCAB.Common;
using UKMCAB.Data;
using UKMCAB.Web.UI.Middleware.BasicAuthentication;
using UKMCAB.Web.UI.Services;

var builder = WebApplication.CreateBuilder(args);
var basicAuthenticationOptions = new BasicAuthenticationOptions().Pipe(x => builder.Configuration.Bind("BasicAuthentication", x));

builder.Services.AddSingleton(basicAuthenticationOptions);
builder.Services.AddTransient<ISearchFilterService, SearchFilterService>();
builder.Services.AddTransient<ICABSearchService, CABSearchService>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddHostedService<UmbracoDataRefreshBackgroundService>();

// =================================================================================================

var app = builder.Build();
app.UseMiddleware<BasicAuthenticationMiddleware>();
app.UseStaticFiles();
app.MapRazorPages();
app.MapDefaultControllerRoute();
app.MapControllers();

CabRepository.Config = builder.Configuration; // todo: use ioc etc.
await CabRepository.LoadAsync(); // todo: use ioc

app.Run();
