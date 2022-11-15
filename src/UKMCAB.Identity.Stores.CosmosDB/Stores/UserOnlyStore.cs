using Microsoft.AspNetCore.Identity;
using UKMCAB.Identity.Stores.CosmosDB.Tables.Interfaces;

namespace UKMCAB.Identity.Stores.CosmosDB.Stores
{
    internal sealed class UserOnlyStore<TUser, TUserClaim, TUserLogin, TUserToken> : UserStoreBase<TUser, TUserClaim, TUserLogin, TUserToken>
        where TUser : IdentityUser<string>, new()
        where TUserClaim : IdentityUserClaim<string>, new()
        where TUserLogin : IdentityUserLogin<string>, new()
        where TUserToken : IdentityUserToken<string>, new()
    {
        public UserOnlyStore(IUsersTable<TUser, string> usersTable,
            IUserLoginsTable<TUser, TUserLogin, string> userLoginsTable,
            IUserClaimsTable<TUser, TUserClaim, string> userClaimsTable,
            IUserTokensTable<TUserToken, string> userTokensTable)
            : base(usersTable, userLoginsTable, userClaimsTable, userTokensTable)
        {
        }

    }
}
