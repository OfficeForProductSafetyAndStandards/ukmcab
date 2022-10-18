using Microsoft.Azure.Cosmos;
using UKMCAB.Common;
using UKMCAB.Data;
using UKMCAB.Data.CosmosDb;
using UKMCAB.Web.UI.Middleware.BasicAuthentication;
using UKMCAB.Web.UI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddSingleton(new BasicAuthenticationOptions() { Password = builder.Configuration["BasicAuthPassword"] });
builder.Services.AddTransient<ISearchFilterService, SearchFilterService>();
builder.Services.AddTransient<ICABSearchService, CABSearchService>();
builder.Services.AddHostedService<UmbracoDataRefreshBackgroundService>();

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

CabRepository.Config = builder.Configuration; // todo: use ioc etc.
//await CabRepository.LoadAsync(); // todo: use ioc

app.Run();
