using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Notify.Interfaces;
using UKMCAB.Core.Domain.Workflow;
using UKMCAB.Core.EmailTemplateOptions;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Core.Services.Workflow;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Areas.Admin.Controllers;
using UKMCAB.Web.UI.Models.ViewModels.Search.RequestToUnarchiveCAB;
using UKMCAB.Web.UI.Models.ViewModels.Shared;
using System.Linq;
using UKMCAB.Core.Domain.CAB;
using Microsoft.AspNetCore.Authorization;
using Org.BouncyCastle.Asn1.Ocsp;
using UKMCAB.Common;

namespace UKMCAB.Web.UI.Areas.Search.Controllers;

[Area("search"), Route("search/governmentusernote"), Authorize]
public class UserNoteController : Controller
{
    private readonly IUserNoteService _userNoteService;
    private readonly IUserService _userService;

    public static class Routes
    {
        public const string GovernmentUserNoteView = "cab.government-user-note.view";
        public const string GovernmentUserNoteCreate = "cab.government-user-note.create";
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

        if (userNote == null)
        {
            //TODO guard in service or check here instead???
        }

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
            ReturnUrl = returnUrl,
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
            ReturnUrl = returnUrl,
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

    [HttpPost("Delete", Name = Routes.GovernmentUserNoteDelete)]
    public async Task<IActionResult> Delete(UserNoteViewModel vm)
    {
        await _userNoteService.DeleteUserNote(vm.CabDocumentId, vm.Id);

        return Redirect(vm.ReturnUrl);
    }
}