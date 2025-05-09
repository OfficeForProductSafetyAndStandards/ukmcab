using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;


namespace UKMCAB.Subscriptions.Core.Data.Models.EntityConfigurations;

public class TableEntityConfiguration : IEntityTypeConfiguration<TableEntity>
{
    public void Configure(EntityTypeBuilder<TableEntity> builder)
    {
        builder.ToTable("table_entities");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).HasColumnType("uuid").IsRequired();

        builder.Property<Dictionary<string, object>>("_properties")
               .HasColumnName("properties")
               .HasColumnType("jsonb")
               .HasConversion(
                   v => JsonSerializer.Serialize(v, new JsonSerializerOptions { WriteIndented = false }),
                   v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }))
               .Metadata.SetField("_properties");

        // Optional: Ignore all other facade properties (EF might try to map them otherwise)
        builder.Ignore(e => e.PartitionKey);
        builder.Ignore(e => e.RowKey);
        builder.Ignore(e => e.Timestamp);
        builder.Ignore(e => e.TableKey);
    }
}
