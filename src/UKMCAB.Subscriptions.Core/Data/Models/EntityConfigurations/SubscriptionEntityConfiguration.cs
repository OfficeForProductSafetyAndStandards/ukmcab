using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UKMCAB.Subscriptions.Core.Data.Models.EntityConfigurations;

public class SubscriptionEntityConfiguration : IEntityTypeConfiguration<SubscriptionEntity>
{
    public void Configure(EntityTypeBuilder<SubscriptionEntity> builder)
    {
        builder.ToTable("subscriptions");

        builder.HasKey(e => new { e.TableKey, e.PartitionKey, e.RowKey });

        builder.Property(e => e.TableKey)
               .HasColumnName("table_key");

        builder.Property(e => e.PartitionKey)
               .HasColumnName("partition_key")
               .IsRequired();

        builder.Property(e => e.RowKey)
               .HasColumnName("row_key")
               .IsRequired();

        builder.Property(e => e.Timestamp)
               .HasColumnName("timestamp");

        builder.Property(e => e.EmailAddress)
               .HasColumnName("email_address")
               .IsRequired();

        builder.Property(e => e.Frequency)
               .HasColumnName("frequency")
               .HasConversion<string>()
               .IsRequired();

        builder.Property(e => e.CabId)
               .HasColumnName("cab_id");

        builder.Property(e => e.CabName)
               .HasColumnName("cab_name");

        builder.Property(e => e.SearchQueryString)
               .HasColumnName("search_query_string");

        builder.Property(e => e.SearchKeywords)
               .HasColumnName("search_keywords");

        builder.Property(e => e.DueBaseDate)
               .HasColumnName("due_base_date");

        builder.Property(e => e.LastThumbprint)
               .HasColumnName("last_thumbprint");

        builder.Property(e => e.BlobName)
               .HasColumnName("blob_name");
    }
}
