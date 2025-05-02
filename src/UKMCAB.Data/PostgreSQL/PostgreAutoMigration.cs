using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace UKMCAB.Data.PostgreSQL;

public static class PostgreAutoMigration
{
    public static void MigrateDatabase(IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var iServiceScopeFactory = app.ApplicationServices.GetService<IServiceScopeFactory>();
        if (iServiceScopeFactory == null)
        {
            throw new ApplicationException($"{nameof(IServiceScopeFactory)} not found");
        }

        using var scope = iServiceScopeFactory.CreateScope();

        scope.ServiceProvider.GetRequiredService<ApplicationDataContext>().Database.Migrate();
    }
}
