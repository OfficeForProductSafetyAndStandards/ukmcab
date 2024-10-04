using NUnit.Framework;
using NUnit.Framework.Legacy;
using UKMCAB.Common.ConnectionStrings;

namespace UKMCAB.Common.Tests;

[TestFixture]
public class CognitiveSearchConnectionStringTest
{
    [Test]
    public void CognitiveSearchConnectionString_Parse()
    {
        var ep = "myinstance.search.windows.net";
        var apikey = "dmdqYndyZWlnaCA1Mjg5NmZieW51M3htNGl4Y20yMy0waSw==";
        var str = $"endpoint={ep};apikey={apikey}";
        var cognitiveSearchConnectionString = new CognitiveSearchConnectionString(str);

        ClassicAssert.That(cognitiveSearchConnectionString.Endpoint,Is.EqualTo(ep));
        ClassicAssert.That(cognitiveSearchConnectionString.ApiKey,Is.EqualTo(apikey));
    }

    [Test]
    public void CognitiveSearchConnectionString_Empty()
    {
        var cognitiveSearchConnectionString = new CognitiveSearchConnectionString(null);
    }
}
