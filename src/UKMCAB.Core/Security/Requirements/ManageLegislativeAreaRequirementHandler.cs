using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
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

            var cabId = ((HttpContext)context.Resource)?.Request.RouteValues["Id"]?.ToString();

            if (cabId is null)
            {
                return Task.CompletedTask;
            }

            var document = _cabAdminService.GetLatestDocumentAsync(cabId).Result;

            if (document is null)
            {
                return Task.CompletedTask;
            }

            var isOpss = user.IsInRole(Roles.OPSS.Id);
            var isOgd = user.IsInRole(Roles.OPSS_OGD.Id);
            
            // if any of the document's LAs are Approved, then it's approved by OGD
            var isApprovedByOGD =  document.DocumentLegislativeAreas.Any(la => la.Status == LAStatus.Approved);
            var isPendingApprovalByOGD = document.IsPendingOgdApproval();

            if (isOgd && isPendingApprovalByOGD)
                context.Succeed(requirement);

            if(isOpss && isApprovedByOGD)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
