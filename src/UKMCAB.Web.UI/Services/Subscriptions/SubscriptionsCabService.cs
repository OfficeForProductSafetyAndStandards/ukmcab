using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using System.Globalization;
using UKMCAB.Core.Services;
using UKMCAB.Data.Search.Services;
using UKMCAB.Subscriptions.Core.Integration.CabService;
using UKMCAB.Web.UI.Areas.Search.Controllers;
using UKMCAB.Web.UI.Models.ViewModels.Search;

namespace UKMCAB.Web.UI.Services.Subscriptions;

/// <summary>
/// Provides search results and CAB data to the Subscriptions Core.
/// </summary>
public class SubscriptionsCabService : ICabService
{
    private readonly ICachedSearchService _cachedSearchService;
    private readonly ICachedPublishedCabService _cachedPublishedCabService;
    private readonly IServiceProvider _services;

    public SubscriptionsCabService(ICachedSearchService cachedSearchService, ICachedPublishedCabService cachedPublishedCabService, IServiceProvider services)
    {
        _cachedSearchService = cachedSearchService;
        _cachedPublishedCabService = cachedPublishedCabService;
        _services = services;
    }

    public async Task<SubscriptionsCoreCabModel?> GetAsync(Guid id)
    {
        var cabDocument = await _cachedPublishedCabService.FindPublishedDocumentByCABIdAsync(id.ToString());
        if (cabDocument != null)
        {
            var cab = new SubscriptionsCoreCabModel
            {
                CABId = cabDocument.CABId,
                PublishedDate = cabDocument.PublishedDate,
                LastModifiedDate = cabDocument.LastUpdatedDate,
                Name = cabDocument.Name,
                UKASReferenceNumber = string.Empty,
                Address = StringExt.Join(", ", cabDocument.AddressLine1, cabDocument.AddressLine2, cabDocument.TownCity, cabDocument.Postcode),
                Website = cabDocument.Website,
                Email = cabDocument.Email,
                Phone = cabDocument.Phone,
                BodyNumber = cabDocument.CABNumber,
                BodyTypes = cabDocument.BodyTypes ?? new List<string>(),
                RegisteredOfficeLocation = cabDocument.RegisteredOfficeLocation,
                RegisteredTestLocations = cabDocument.TestingLocations ?? new List<string>(),
                LegislativeAreas = cabDocument.LegislativeAreas ?? new List<string>(),
                ProductSchedules = cabDocument.Schedules?.Select(pdf => new SubscriptionsCoreCabFileModel
                {
                    BlobName = pdf.BlobName,
                    FileName = pdf.FileName
                }).ToList() ?? new List<SubscriptionsCoreCabFileModel>(),
            };
            return cab;
        }
        else
        {
            return null;
        }
    }

    public async Task<CabApiService.SearchResults> SearchAsync(string? query)
    {
        var model = await BindAsync<SearchViewModel>(query).ConfigureAwait(false);
        var data = await SearchController.SearchInternalAsync(_cachedSearchService, model ?? new SearchViewModel(), x => x.IgnorePaging = true);
        return new CabApiService.SearchResults(data.Total, data.CABs.Select(x => new SubscriptionsCoreCabSearchResultModel { CabId = Guid.Parse(x.CABId), Name = x.Name }).ToList());
    }

    private async Task<T?> BindAsync<T>(string? query)
    {
        var factory = _services.GetRequiredService<IModelBinderFactory>();
        var metadataProvider = _services.GetRequiredService<IModelMetadataProvider>();

        var metadata = metadataProvider.GetMetadataForType(typeof(T));
        var modelBinder = factory.CreateBinder(new()
        {
            Metadata = metadata
        });

        var context = new DefaultModelBindingContext
        {
            ModelMetadata = metadata,
            ModelName = string.Empty,
            ValueProvider = new QueryStringValueProvider(
                BindingSource.Query,
                new QueryCollection(QueryHelpers.ParseQuery(query)),
                CultureInfo.InvariantCulture
            ),
            ActionContext = new ActionContext(
                new DefaultHttpContext(),
                new RouteData(),
                new ActionDescriptor()),
            ModelState = new ModelStateDictionary()
        };
        await modelBinder.BindModelAsync(context);
        return (T?)context.Result.Model;
    }


    public void Dispose() => GC.SuppressFinalize(this);
}
