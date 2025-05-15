using UKMCAB.Subscriptions.Core.Common;
using UKMCAB.Subscriptions.Core.Integration.CabService;

namespace UKMCAB.Subscriptions.Test.Integration;

[Explicit]
public class CabServiceTest
{

    [Test]
    public async Task TestSearch()
    {
        var subj = CreateCabService();
        var results = await subj.SearchAsync(null).ConfigureAwait(false);
        Assert.That(results, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(results.Total, Is.GreaterThan(0));
            Assert.That(results.Results.Count, Is.GreaterThan(0));
            Assert.That(results.Results.All(x => x.Name != null));
        });
    }

    //[Test]
    //public async Task TestGetCab()
    //{
    //    var subj = CreateCabService();
    //    var result = await subj.GetAsync(Guid.Parse("ba3968f4-ff0e-4993-b47e-9ca4de55f98d")).ConfigureAwait(false);
    //    Assert.That(result, Is.Not.Null);
    //    Assert.Multiple(() =>
    //    {
    //        Assert.That(result.Name, Is.EqualTo("BSI Assurance UK Ltd"));
    //    });
    //}

    private static CabApiService CreateCabService() => new CabApiService(new CabApiOptions(new Uri("http://localhost:7060/"), BasicAuthenticationHeaderValue.Create("internal", "bob")));
}
