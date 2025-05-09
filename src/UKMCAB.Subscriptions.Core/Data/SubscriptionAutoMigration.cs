using Microsoft.EntityFrameworkCore;

namespace UKMCAB.Subscriptions.Core.Data;

public static class SubscriptionAutoMigration
{
    public static void MigrateDatabase(IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var iServiceScopeFactory = app.ApplicationServices.GetService<IServiceScopeFactory>();
        if (iServiceScopeFactory == null)
        {
            throw new ApplicationException($"{nameof(IServiceScopeFactory)} not found");
        }

        var scope = iServiceScopeFactory.CreateScope();

        scope.ServiceProvider.GetRequiredService<SubscriptionDbContext>().Database.Migrate();
    }
}
