using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UKMCAB.Data.Search.Services
{
    /// <summary>
    /// TEMPORARY Open Search Indexer implementation (update as required)
    /// </summary>
    public class OpenSearchIndexerClient : IOpenSearchIndexerClient
    {
        public async Task RunIndexerAsync(string indexerName, CancellationToken token = default)
        {
            await Task.CompletedTask;
        }
    }
}
