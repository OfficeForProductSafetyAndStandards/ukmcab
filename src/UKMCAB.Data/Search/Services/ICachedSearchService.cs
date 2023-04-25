namespace UKMCAB.Data.Search.Services
{
    public interface ICachedSearchService : ISearchService
    {
        /// <summary>
        /// Clears down all search result cache items that contain a particular CAB.
        /// </summary>
        /// <param name="cabId"></param>
        /// <returns></returns>
        /// <remarks>
        ///     This is to be used where a CAB is updated and you cannot know which CAB search result cache items contain a given CAB.
        /// </remarks>
        Task ClearAsync(string cabId);

        /// <summary>
        /// Clears all cached searches.  Useful for when a CAB is published/unpublished.
        /// Remember the CAB will need to have been index first, so may need to manually update the search index, before clearing cache items.
        /// </summary>
        /// <returns></returns>
        Task ClearAsync();
    }
}
