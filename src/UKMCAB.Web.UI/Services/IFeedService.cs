using System.ServiceModel.Syndication;
using UKMCAB.Data.Models;
using UKMCAB.Data.Search.Models;

namespace UKMCAB.Web.UI.Services
{
    public interface IFeedService
    {
        SyndicationFeed GetSyndicationFeed(string? feedName, HttpRequest request, IEnumerable<CABIndexItem> items, IUrlHelper url);
        SyndicationFeed GetSyndicationFeed(string? feedName, HttpRequest request, Document item, IUrlHelper url);
    }
}
