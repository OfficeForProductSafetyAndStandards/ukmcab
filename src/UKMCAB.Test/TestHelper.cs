using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using UKMCAB.Identity.Stores.CosmosDB;

namespace UKMCAB.Test;

public class TestHelper
{
    public static void SetupUrlActionReturn(Controller controller)
    {
        // HttpContext.Request needs setting up
        var mockRequest = new Mock<HttpRequest>();
        mockRequest.SetupGet(r => r.Host).Returns(It.IsAny<HostString>());
        mockRequest.Setup(r => r.Headers).Returns(new HeaderDictionary());
        mockRequest.SetupGet(r => r.Scheme).Returns(It.IsAny<string>());
        var context = new Mock<HttpContext>();
        context.SetupGet(c => c.Request).Returns(mockRequest.Object);
        controller.ControllerContext.HttpContext = context.Object;
        // UrlHelper needs setting up
        var mockUrlHelper = new Mock<IUrlHelper>(MockBehavior.Strict);
        mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns("callbackUrl").Verifiable();
        controller.Url = mockUrlHelper.Object;
    }
}