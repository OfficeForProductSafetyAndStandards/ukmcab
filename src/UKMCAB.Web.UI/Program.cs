using Microsoft.Azure.Cosmos;
using UKMCAB.Common;
using UKMCAB.Data;
using UKMCAB.Data.CosmosDb;
using UKMCAB.Web.UI.Middleware.BasicAuthentication;
using UKMCAB.Web.UI.Services;

var builder = WebApplication.CreateBuilder(args);
var basicAuthenticationOptions = new BasicAuthenticationOptions().Pipe(x => builder.Configuration.Bind("BasicAuthentication", x));

builder.Services.AddControllersWithViews();

builder.Services.AddSingleton(basicAuthenticationOptions);
builder.Services.AddTransient<ISearchFilterService, SearchFilterService>();
builder.Services.AddTransient<ICABSearchService, CABSearchService>();
builder.Services.AddHostedService<UmbracoDataRefreshBackgroundService>();

var cosmosDbSettings = builder.Configuration.GetSection("CosmosDb");
var account = cosmosDbSettings.GetValue<string>("Account");
var key = cosmosDbSettings.GetValue<string>("Key");
var database = cosmosDbSettings.GetValue<string>("Database");
var container = cosmosDbSettings.GetValue<string>("Container");
builder.Services.AddSingleton<ICosmosDbService>(new CosmosDbService(new CosmosClient(account, key), database, container));

// =================================================================================================

var app = builder.Build();

app.UseMiddleware<BasicAuthenticationMiddleware>();

app.UseStaticFiles();

app.MapDefaultControllerRoute();

CabRepository.Config = builder.Configuration; // todo: use ioc etc.
await CabRepository.LoadAsync(); // todo: use ioc

app.Run();
