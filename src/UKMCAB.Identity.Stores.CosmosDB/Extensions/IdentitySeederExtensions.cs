using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Security.Claims;
using UKMCAB.Common.ConnectionStrings;

namespace UKMCAB.Identity.Stores.CosmosDB.Extensions
{
    public static class IdentitySeederExtensions
    {
        public static async Task<IApplicationBuilder> InitialiseIdentitySeedingAsync<TUser, TRole>(this IApplicationBuilder app, AzureDataConnectionString azureDataConnectionString, string dataProtectionKeysContainerName, 
            Action<IdentitySeeds<TUser, TRole>> seeds)
            where TUser : IdentityUser<string>
            where TRole : IdentityRole<string>
        {
            await new BlobContainerClient(azureDataConnectionString, dataProtectionKeysContainerName).CreateIfNotExistsAsync(); // ensure the data protection keys blob container is created.

            IdentitySeeds<TUser, TRole> identitySeeds = new();
            seeds(identitySeeds);
            using (var scope = app.ApplicationServices.CreateScope())
            {
                RoleManager<TRole>? roleManager = scope.ServiceProvider.GetService<RoleManager<TRole>>();
                if (roleManager is not null)
                {
                    await AddRolesAsync(roleManager, identitySeeds);
                }

                await AddUsersAsync(scope.ServiceProvider.GetRequiredService<UserManager<TUser>>(), roleManager, identitySeeds);
            }
            return app;
        }

        private static async Task AddRolesAsync<TUser, TRole>(RoleManager<TRole> roleManager, IdentitySeeds<TUser, TRole> identitySeeds)
            where TUser : IdentityUser<string>
            where TRole : IdentityRole<string>
        {
            foreach (var role in identitySeeds.Roles)
            {
                if (!await roleManager.RoleExistsAsync(role.Role.Name))
                {
                    IdentityResult result = await roleManager.CreateAsync(role.Role);
                    if (result.Succeeded)
                    {
                        foreach (Claim claim in role.Claims)
                            await roleManager.AddClaimAsync(role.Role, claim);
                    }
                }
            }
        }

        private static async Task AddUsersAsync<TUser, TRole>(UserManager<TUser> userManager, RoleManager<TRole>? roleManager, IdentitySeeds<TUser, TRole> identitySeeds)
            where TUser : IdentityUser<string>
            where TRole : IdentityRole<string>
        {
            foreach (var user in identitySeeds.Users)
            {
                if (await userManager.FindByEmailAsync(user.User.Email) == null)
                {
                    IdentityResult result = await userManager.CreateAsync(user.User, user.Password);
                    if (result.Succeeded)
                    {
                        if (user.Claims?.Any() == true)
                        {
                            foreach (Claim claim in user.Claims)
                            {
                                await userManager.AddClaimAsync(user.User, claim);
                            }
                        }
                        if (roleManager is not null && user.Roles?.Any() == true)
                        {
                            foreach (TRole role in user.Roles)
                            {
                                if (!identitySeeds.Roles.Any(i => i.Role.Name == role.Name) && !await roleManager.RoleExistsAsync(role.Name))
                                {
                                    await roleManager.CreateAsync(role);
                                }
                                await userManager.AddToRoleAsync(user.User, role.Name);
                            }
                        }
                    }
                }
            }
        }
    }

    public class IdentitySeeds<TUser, TRole>
        where TUser : IdentityUser<string>
        where TRole : IdentityRole<string>
    {
        internal IdentitySeeds()
        {

        }

        internal Collection<UserDescriptor> Users { get; } = new();
        internal Collection<RoleDescriptor> Roles { get; } = new();

        public IdentitySeeds<TUser, TRole> AddUser(TUser user, string password, IEnumerable<Claim>? claims = null, IEnumerable<TRole>? roles = null)
        {
            Users.Add(new(user, password, claims, roles));
            return this;
        }
        public IdentitySeeds<TUser, TRole> AddUser(TUser user, string password, params Claim[] claims) => AddUser(user, password, claims, null);
        public IdentitySeeds<TUser, TRole> AddUser(TUser user, string password, params TRole[] roles) => AddUser(user, password, null, roles);

        public IdentitySeeds<TUser, TRole> AddRole(TRole role, params Claim[] claims)
        {
            Roles.Add(new(role, claims));
            return this;
        }

        internal record UserDescriptor(TUser User, string Password, IEnumerable<Claim>? Claims, IEnumerable<TRole>? Roles);
        internal record RoleDescriptor(TRole Role, IEnumerable<Claim> Claims);
    }
}
