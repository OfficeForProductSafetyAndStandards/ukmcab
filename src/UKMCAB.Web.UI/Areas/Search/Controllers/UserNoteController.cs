using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Models.ViewModels.Shared;

namespace UKMCAB.Web.UI.Areas.Search.Controllers;

[Area("search"), Route("search/governmentusernote"), Authorize(Policy = Policies.GovernmentUserNotes)]
public class UserNoteController : Controller
{
    private readonly IUserNoteService _userNoteService;
    private readonly IUserService _userService;

    public static class Routes
    {
        public const string GovernmentUserNoteView = "cab.government-user-note.view";
        public const string GovernmentUserNoteCreate = "cab.government-user-note.create";
        public const string GovernmentUserNoteConfirmDelete = "cab.government-user-note.confirm";
        public const string GovernmentUserNoteDelete = "cab.government-user-note.delete";
    }

    public UserNoteController(IUserNoteService userNoteService, IUserService userService)
    {
        _userNoteService = userNoteService;
        _userService = userService;
    }

    [HttpGet("View", Name = Routes.GovernmentUserNoteView)]
    public async Task<IActionResult> View(Guid cabDocumentId, Guid userNoteId, string returnUrl)
    {
        UserNote userNote = await _userNoteService.GetUserNote(cabDocumentId, userNoteId);

        var vm = new UserNoteViewModel()
        {
            Title = "Government user note",
            Id = userNoteId,
            CabDocumentId = cabDocumentId,
            DateAndTime = userNote.DateTime,
            UserId = userNote.UserId,
            UserName = userNote.UserName,
            UserGroup = userNote.UserRole,
            Note = userNote.Note,
            ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : "/",
            IsOPSSOrInCreatorUserGroup = User.IsInRole(Roles.OPSS.Id) || User.IsInRole(userNote.UserRole),
        };

        return View(vm);
    }

    [HttpGet("Create", Name = Routes.GovernmentUserNoteCreate)]
    public async Task<IActionResult> Create(Guid cabDocumentId, string returnUrl)
    {
        var vm = new UserNoteCreateViewModel ()
        {
            Title = "Government user note",
            CabDocumentId = cabDocumentId,
            ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : "/",
        };

        return View(vm);
    }

    [HttpPost("Create", Name = Routes.GovernmentUserNoteCreate)]
    public async Task<IActionResult> Create(UserNoteCreateViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var currentUser = await _userService.GetAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)) ??
                          throw new InvalidOperationException();

        await _userNoteService.CreateUserNote(currentUser, vm.CabDocumentId, vm.Note);

        return Redirect(vm.ReturnUrl);
    }

    [HttpGet("ConfirmDelete", Name = Routes.GovernmentUserNoteConfirmDelete)]
    public async Task<IActionResult> ConfirmDelete(Guid cabDocumentId, Guid userNoteId, string returnUrl, string backUrl)
    {
        UserNote userNote = await _userNoteService.GetUserNote(cabDocumentId, userNoteId);

        var vm = new UserNoteViewModel()
        {
            Title = "Delete government user note",
            Id = userNoteId,
            CabDocumentId = cabDocumentId,
            DateAndTime = userNote.DateTime,
            UserId = userNote.UserId,
            UserName = userNote.UserName,
            UserGroup = userNote.UserRole,
            Note = userNote.Note,
            ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : "/",
            BackUrl = backUrl,
            IsOPSSOrInCreatorUserGroup = User.IsInRole(Roles.OPSS.Id) || User.IsInRole(userNote.UserRole),
        };

        return View(vm);
    }

    [HttpPost("Delete", Name = Routes.GovernmentUserNoteDelete)]
    public async Task<IActionResult> Delete(UserNoteViewModel vm)
    {
        await _userNoteService.DeleteUserNote(vm.CabDocumentId, vm.Id);

        return Redirect(vm.ReturnUrl);
    }
}