using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;
using UKMCAB.Identity.Stores.CosmosDB.Stores;

namespace UKMCAB.Identity.Stores.CosmosDB.Extensions
{
    public static class IdentityStoresBuilderExtensions
    {
        public static void AddStores<TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim>(this IdentityBuilder builder)
            where TUserClaim : IdentityUserClaim<string>, new()
            where TUserRole : IdentityUserRole<string>, new()
            where TUserLogin : IdentityUserLogin<string>, new()
            where TUserToken : IdentityUserToken<string>, new()
            where TRoleClaim : IdentityRoleClaim<string>, new()
        {
            if (!TryFindGenericBaseType(builder.UserType, typeof(IdentityUser<>), out Type? _))
            {
                throw new InvalidOperationException("AddEntityFrameworkStores can only be called with a user that derives from IdentityUser<TKey>.");
            }

            if (builder.RoleType != null)
            {
                if (!TryFindGenericBaseType(builder.RoleType, typeof(IdentityRole<>), out Type? _))
                {
                    throw new InvalidOperationException("AddEntityFrameworkStores can only be called with a role that derives from IdentityRole<TKey>.");
                }

                Type userStoreType = typeof(UserStore<,,,,,>).MakeGenericType(builder.UserType, builder.RoleType, typeof(TUserClaim), typeof(TUserRole), typeof(TUserLogin), typeof(TUserToken));

                Type roleStoreType = typeof(RoleStore<,>).MakeGenericType(builder.RoleType, typeof(TRoleClaim));

                builder.Services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(builder.UserType), userStoreType);
                builder.Services.TryAddScoped(typeof(IRoleStore<>).MakeGenericType(builder.RoleType), roleStoreType);
            }
            else
            {   // No Roles
                Type userStoreType = typeof(UserOnlyStore<,,,>).MakeGenericType(builder.UserType, typeof(TUserClaim), typeof(TUserLogin), typeof(TUserToken));
                builder.Services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(builder.UserType), userStoreType);
            }

        }

        private static bool TryFindGenericBaseType(Type currentType, Type genericBaseType, [NotNullWhen(true)] out Type? type)
        {
            type = currentType;
            while (type != null)
            {
                var genericType = type.IsGenericType ? type.GetGenericTypeDefinition() : null;
                if (genericType != null && genericType == genericBaseType)
                {
                    type = currentType;
                    return true;
                }
                type = type.BaseType;
            }
            type = null;
            return false;
        }
    }
}
