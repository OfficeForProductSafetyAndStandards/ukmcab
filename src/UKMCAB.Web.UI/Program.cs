using Microsoft.Azure.Cosmos;
using UKMCAB.Data.CosmosDb;
using UKMCAB.Data.CosmosDb.Services;
using UKMCAB.Web.UI.Middleware.BasicAuthentication;
using UKMCAB.Web.UI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddSingleton(new BasicAuthenticationOptions() { Password = builder.Configuration["BasicAuthPassword"] });
builder.Services.AddTransient<ISearchFilterService, SearchFilterService>();
builder.Services.AddTransient<ICABSearchService, CABSearchService>();

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

app.UseMiddleware<BasicAuthenticationMiddleware>();

app.UseStaticFiles();

app.MapDefaultControllerRoute();

app.Run();
