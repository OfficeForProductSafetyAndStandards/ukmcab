using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UKMCAB.Data.Search.Services
{
    /// <summary>
    /// TEMPORARY Open Search Indexer (update as required)
    /// </summary>
    public interface IOpenSearchIndexerClient
    {
        Task RunIndexerAsync(string indexerName, CancellationToken token = default);
    }
}
