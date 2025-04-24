using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using UKMCAB.Common;

namespace UKMCAB.Web;

public interface IAppHost
{
    Uri GetBaseUri();
}

public class AppHost : IAppHost
{
    private readonly IConfiguration _configuration;
    private readonly IServer _server;

    public AppHost(IConfiguration configuration, IServer server)
    {
        _configuration = configuration;
        _server = server;
    }

    public Uri GetBaseUri()
    {
        return new Uri("http://localhost:57977/");
        //var defaultAppAddresses = _server.Features.Get<IServerAddressesFeature>()?.Addresses.ToArray() ?? Array.Empty<string>();
        //var defaultAppAddress = defaultAppAddresses.FirstOrDefault(x => x.StartsWith("https"));
        //var configuredAppAddress = _configuration["AppHostName"].PrependIf("https://");
        //var baseAddress = configuredAppAddress ?? defaultAppAddress ?? throw new Exception("No web addresses obtainable");
        //var @base = new Uri(baseAddress);
        //return @base;
    }
}
