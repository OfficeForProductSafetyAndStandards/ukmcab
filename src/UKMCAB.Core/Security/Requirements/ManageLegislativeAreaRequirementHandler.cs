using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;
using UKMCAB.Common.Extensions;
using UKMCAB.Core.Extensions;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Data.Models;

namespace UKMCAB.Core.Security.Requirements
{
    public class ManageLegislativeAreaRequirementHandler : AuthorizationHandler<ManageLegislativeAreaRequirement>
    {
        private readonly ICABAdminService _cabAdminService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ManageLegislativeAreaRequirementHandler(
            ICABAdminService cabAdminService,
            IHttpContextAccessor httpContextAccessor)
        {
            _cabAdminService = cabAdminService;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, ManageLegislativeAreaRequirement requirement)
        {
            var user = context.User;

            var cabId = (_httpContextAccessor.HttpContext!.GetRouteValue("id")?.ToString()) ?? throw new InvalidOperationException("CAB Id not found in route");

            var document = _cabAdminService.GetLatestDocumentAsync(cabId).Result;

            if (document is null)
            {
                return Task.CompletedTask;
            }

            var isOpss = user.IsInRole(Roles.OPSS.Id);
            var isOgd = user.HasOgdRole();
            var isUkas = user.IsInRole(Roles.UKAS.Id);

            var userInCreatorUserGroup = user.IsInRole(document.CreatedByUserGroup);

            var canBeAccessedByOpss = CanOpssAccess(document, isOpss, userInCreatorUserGroup);
            var canBeAccessedByUkas = CanUkasAccess(document, isUkas, userInCreatorUserGroup);
            var canBeAccessedByOgd = CanOgdAccess(user, document, isOgd);

            if (canBeAccessedByOpss || canBeAccessedByOgd || canBeAccessedByUkas)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

        private static bool CanOgdAccess(ClaimsPrincipal user, Document document, bool isOgd) => isOgd &&
                            document.StatusValue == Status.Draft &&
                            document.SubStatus == SubStatus.PendingApprovalToPublish &&
                            document.HasActionableLegislativeAreaForOgd(user.GetRoleId());     
        private static bool CanOpssAccess(Document document, bool isOpss, bool userInCreatorUserGroup) => isOpss && (
                        (userInCreatorUserGroup &&
                        document.StatusValue == Status.Draft &&
                        document.SubStatus == SubStatus.None) ||
                        (document.SubStatus == SubStatus.PendingApprovalToPublish && document.HasActionableLegislativeAreaForOpssAdmin()));
        private static bool CanUkasAccess(Document document, bool isUkas, bool userInCreatorUserGroup) => isUkas &&
                            userInCreatorUserGroup &&
                            document.StatusValue == Status.Draft &&
                            document.SubStatus == SubStatus.None;        
    }
}
