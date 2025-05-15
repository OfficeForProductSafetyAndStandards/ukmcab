using Microsoft.EntityFrameworkCore;
using UKMCAB.Subscriptions.Core.Data.Models;

namespace UKMCAB.Subscriptions.Core.Data;

public class SubscriptionDbContext : DbContext
{
    public SubscriptionDbContext(DbContextOptions<SubscriptionDbContext> options)
        : base(options)
    {
    }

    public DbSet<TableEntity> TableEntities => Set<TableEntity>();
    public DbSet<SubscriptionEntity> SubscriptionEntities => Set<SubscriptionEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SubscriptionEntity>()
            .HasKey(e => new { e.TableKey, e.PartitionKey, e.RowKey });
        modelBuilder.Entity<TableEntity>()
            .HasKey(e => new { e.TableKey, e.PartitionKey, e.RowKey });
    }
}

