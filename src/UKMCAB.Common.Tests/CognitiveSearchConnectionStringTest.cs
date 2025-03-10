﻿using NUnit.Framework;
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

        Assert.That(cognitiveSearchConnectionString.Endpoint,Is.EqualTo(ep));
        Assert.That(cognitiveSearchConnectionString.ApiKey,Is.EqualTo(apikey));
    }

    [Test]
    public void CognitiveSearchConnectionString_Empty()
    {
        var cognitiveSearchConnectionString = new CognitiveSearchConnectionString(null);
    }
}
