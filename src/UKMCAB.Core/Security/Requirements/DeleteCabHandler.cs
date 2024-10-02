using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Data.Models;
using UKMCAB.Core.Extensions;

namespace UKMCAB.Core.Security.Requirements
{
    public class DeleteCabHandler : AuthorizationHandler<DeleteCabRequirement>
    {
        private readonly ICABAdminService _cabAdminService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DeleteCabHandler(
            ICABAdminService cabAdminService, 
            IHttpContextAccessor httpContextAccessor)
        {
            _cabAdminService = cabAdminService;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, DeleteCabRequirement requirement)
        {
            var user = context.User;

            var cabId = (_httpContextAccessor.HttpContext!.GetRouteValue("cabId")?.ToString()) ?? throw new Exception("CAB Id not found in route");
            var document = _cabAdminService.GetLatestDocumentAsync(cabId).Result;

            if (document?.StatusValue == Status.Draft && document?.SubStatus == SubStatus.None)
            {
                if (user.IsInRole(Roles.OPSS.Id) || user.IsInRole(document.CreatedByUserGroup))
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}
