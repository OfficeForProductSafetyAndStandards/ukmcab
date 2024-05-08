using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Data.Models;
using UKMCAB.Data.Extensions;

namespace UKMCAB.Core.Security.Requirements
{
    public class EditCabPendingApprovalHandler : AuthorizationHandler<EditCabPendingApprovalRequirement>
    {
        private readonly ICABAdminService _cabAdminService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EditCabPendingApprovalHandler(
            ICABAdminService cabAdminService, 
            IHttpContextAccessor httpContextAccessor)
        {
            _cabAdminService = cabAdminService;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, EditCabPendingApprovalRequirement requirement)
        {
            var user = context.User;
            var isOpssAdmin = user.IsInRole(Roles.OPSS.Id);

            var cabId = (_httpContextAccessor.HttpContext!.GetRouteValue("id")?.ToString()) ?? throw new Exception("CAB Id not found in route");
            var document = _cabAdminService.GetLatestDocumentAsync(cabId).Result;

            if (document == null)
            {
                if (isOpssAdmin || user.IsInRole(Roles.UKAS.Id))
                {
                    context.Succeed(requirement);
                }
            }
            else
            {
                var isOpssAdminOrInCreatorUserGroup = isOpssAdmin || user.IsInRole(document.CreatedByUserGroup);
                var legislativeAreaHasBeenActioned = document.DocumentLegislativeAreas.HasBeenActioned();

                if ((document.SubStatus == SubStatus.PendingApprovalToPublish && isOpssAdmin && legislativeAreaHasBeenActioned) ||
                    (document.SubStatus != SubStatus.PendingApprovalToPublish && isOpssAdminOrInCreatorUserGroup))
                {
                    context.Succeed(requirement);
                }
            }
            return Task.CompletedTask;
        }
    }
}
