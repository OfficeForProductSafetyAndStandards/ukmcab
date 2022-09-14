using UKMCAB.Common;
using UKMCAB.Web.UI.Middleware.BasicAuthentication;
using UKMCAB.Web.UI.Services;

var builder = WebApplication.CreateBuilder(args);
var basicAuthenticationOptions = new BasicAuthenticationOptions().Pipe(x => builder.Configuration.Bind("BasicAuthentication", x));

builder.Services.AddSingleton(basicAuthenticationOptions);
builder.Services.AddTransient<ISearchFilterService, SearchFilterService>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// =================================================================================================

var app = builder.Build();
app.UseMiddleware<BasicAuthenticationMiddleware>();
app.UseStaticFiles();
app.MapRazorPages();
app.MapDefaultControllerRoute();
app.MapControllers();
app.Run();
