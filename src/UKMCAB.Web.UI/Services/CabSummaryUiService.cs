using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Security.Claims;
using UKMCAB.Common.Exceptions;
using UKMCAB.Core.Extensions;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;

namespace UKMCAB.Web.UI.Services
{
    public class CabSummaryUiService : ICabSummaryUiService
    {
        private readonly IUserService _userService;
        private readonly ICABAdminService _cabAdminService;
        private readonly IEditLockService _editLockService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;
        private readonly ClaimsPrincipal _user;

        public CabSummaryUiService(IUserService userService, ICABAdminService cabAdminService, IEditLockService editLockService, IHttpContextAccessor httpContextAccessor, ITempDataDictionaryFactory tempDataDictionaryFactory)
        {
            _userService = userService;
            _cabAdminService = cabAdminService;
            _editLockService = editLockService;
            _httpContextAccessor = httpContextAccessor;
            _tempDataDictionaryFactory = tempDataDictionaryFactory;
            _user = _httpContextAccessor.HttpContext?.User ?? throw new InvalidOperationException("No active HttpContext");
        }

        public async Task CreateDocumentAsync(Document document, bool? subSectionEditAllowed)
        {
            var userId = _user.GetUserId();
            if (document.StatusValue == Status.Published && subSectionEditAllowed == true)
            {
                var userAccount = await _userService.GetAsync(userId) ?? throw new NotFoundException($"User account not found for Id: {userId}");
                document = await _cabAdminService.CreateDocumentAsync(userAccount, document);
            }
        }

        public string? GetSuccessBannerMessage()
        {
            string? successBannerMessage = null;

            var tempData = _tempDataDictionaryFactory.GetTempData(_httpContextAccessor.HttpContext);
            if (tempData.ContainsKey(Constants.ApprovedLA))
            {
                tempData.Remove(Constants.ApprovedLA);
                successBannerMessage = "Legislative area has been approved.";
            }

            if (tempData.ContainsKey(Constants.DeclinedLA))
            {
                tempData.Remove(Constants.DeclinedLA);
                successBannerMessage = "Legislative area has been declined.";
            }

            return successBannerMessage;
        }

        public async Task LockCabForUser(CABSummaryViewModel model)
        {
            if (model.SubSectionEditAllowed &&
                model.Status is Status.Draft or Status.Published &&
                model.IsOPSSOrInCreatorUserGroup)
            {
                if (model.CABId == null)
                {
                    throw new InvalidOperationException($"{nameof(CABSummaryViewModel.CABId)} not set on {nameof(CABSummaryViewModel)}");
                }
                await _editLockService.SetAsync(model.CABId, _user.GetUserId());
            }
        }
    }
}
