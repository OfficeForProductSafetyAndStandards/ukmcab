using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;


namespace UKMCAB.Subscriptions.Core.Data.Models.EntityConfigurations;

public class TableEntityConfiguration : IEntityTypeConfiguration<TableEntity>
{
    public void Configure(EntityTypeBuilder<TableEntity> builder)
    {
        builder.ToTable("table_entities");

        builder.HasKey(e => new { e.TableKey, e.PartitionKey, e.RowKey });

        builder.Property(e => e.TableKey)
               .HasColumnName("table_key");

        builder.Property(e => e.PartitionKey)
               .HasColumnName("partition_key")
               .IsRequired();

        builder.Property(e => e.RowKey)
               .HasColumnName("row_key")
               .IsRequired();

        builder.Property<Dictionary<string, object>>("_properties")
               .HasColumnName("properties")
               .HasColumnType("jsonb")
               .HasConversion(
                   v => JsonSerializer.Serialize(v, new JsonSerializerOptions { WriteIndented = false }),
                   v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }))
               .Metadata.SetField("_properties");

        // Optional: Ignore all other facade properties (EF might try to map them otherwise)
        builder.Ignore(e => e.Timestamp);
    }
}
