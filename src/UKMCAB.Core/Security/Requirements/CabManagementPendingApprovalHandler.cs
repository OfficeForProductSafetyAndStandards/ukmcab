using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Data.Models;
using UKMCAB.Data.Extensions;
using UKMCAB.Core.Security.Extensions;

namespace UKMCAB.Core.Security.Requirements
{
    public class CabManagementPendingApprovalHandler : AuthorizationHandler<CabManagementPendingApprovalRequirement>
    {
        private readonly ICABAdminService _cabAdminService;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CabManagementPendingApprovalHandler(
            ICABAdminService cabAdminService, 
            IHttpContextAccessor httpContextAccessor)
        {
            _cabAdminService = cabAdminService;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, CabManagementPendingApprovalRequirement requirement)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                var cabId = (httpContext.GetRouteValue("id")?.ToString()) ?? throw new Exception("CAB Id not found in route");
                var document = _cabAdminService.GetLatestDocumentAsync(cabId).Result ?? throw new Exception($"Document with {cabId} not found");
                
                var userId = httpContext.User.Claims.GetNameIdentifier();
                var userAccount = _userService.GetAsync(userId).Result ?? throw new Exception($"User with {userId} not found");
                
                var isOpssAdmin = userAccount.Role == Roles.OPSS.Id;
                var legislativeAreaHasBeenActioned = document.DocumentLegislativeAreas.HasBeenActioned();

                if (document.SubStatus == SubStatus.PendingApprovalToPublish && isOpssAdmin && legislativeAreaHasBeenActioned)
                {
                    context.Succeed(requirement);
                }
            }
            return Task.CompletedTask;
        }
    }
}
