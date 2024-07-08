using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace UKMCAB.Web.UI.Tests.Areas.Admin.Controllers
{
    public class ControllerBaseTestsBase
    {
        protected ControllerContext GetControllerContextWithUser()
        {
            var userClaims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, "userId"),
            new Claim(ClaimTypes.Role, "roleId"),
            };

            var userIdentity = new ClaimsIdentity(userClaims, "TestAuth");
            var userPrincipal = new ClaimsPrincipal(userIdentity);

            DefaultHttpContext httpContext = new DefaultHttpContext
            {
                User = userPrincipal
            };

            return new ControllerContext
            {
                HttpContext = httpContext
            };
        }
    }
}
