﻿using UKMCAB.Data.Models.Users;
using UKMCAB.Web.UI.Models.ViewModels.Shared;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.User
{
    public class AccountRequestListViewModel : ILayoutModel
    {
        public List<UserAccountRequest> UserAccountRequests { get; set; }
        public string? Title => "User account requests";
        public PaginationViewModel Pagination { get; set; }
        public string? SortField { get; set; }
        public string? SortDirection { get; set; }
        public int ActiveUsersCount { get; set; }
        public int LockedUsersCount { get; set; }
        public int ArchivedUsersCount { get; set; }
    }
}
