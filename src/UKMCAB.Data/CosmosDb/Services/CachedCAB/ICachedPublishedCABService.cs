﻿using UKMCAB.Data.Models;

namespace UKMCAB.Data.CosmosDb.Services.CachedCAB
{
    public interface ICachedPublishedCABService
    {
        Task<Document?> FindPublishedDocumentByCABURLOrGuidAsync(string url);
        Task<Document?> FindPublishedDocumentByCABIdAsync(string id);

        Task<List<Document?>> FindAllDocumentsByCABIdAsync(string cabId);

        Task<Document?> FindDraftDocumentByCABIdAsync(string id);
        Task<int> PreCacheAllCabsAsync();
        Task ClearAsync(string id, string slug);
    }
}
