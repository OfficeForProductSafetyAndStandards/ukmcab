using System;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;

namespace UKMCAB.Common.Tests;

[TestFixture]
public class UriTests
{
    [Theory]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void EmptyPath_GetAbsoluteUriFromRequestAndPath_ThrowsArgumentNullException(string path)
    {
        Mock<HttpContext> httpContext = new Mock<HttpContext>();
        Assert.Throws<ArgumentNullException>(() =>
            UriHelper.GetAbsoluteUriFromRequestAndPath(httpContext.Object.Request, path));
    }
    
    [Test]
    public void Port_GetAbsoluteUriFromRequestAndPath_ReturnsHostAndPort()
    {
        Mock<HttpContext> httpContext = new Mock<HttpContext>();
        httpContext.SetupGet(h => h.Request.Scheme)
            .Returns(() => "https");
        httpContext.SetupGet(h => h.Request.Host)
            .Returns(() => new HostString("test", 99));
        var result = UriHelper.GetAbsoluteUriFromRequestAndPath(httpContext.Object.Request, "/path");
        Assert.AreEqual("https://test:99/path", result);
    }
}