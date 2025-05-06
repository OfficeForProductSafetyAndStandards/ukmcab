
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKMCAB.Data.Models;

namespace UKMCAB.Data.PostgreSQL.EntityConfigurations;

public class CABDocumentBlobEntityTypeConfiguration : IEntityTypeConfiguration<CABDocumentBlob>
{
    public void Configure(EntityTypeBuilder<CABDocumentBlob> builder)
    {
        builder.HasKey(a => a.id);

        builder.Property(a => a.id).HasColumnType("varchar(36)").IsRequired();
        builder.Property(a => a.StatusValue).HasColumnType("text").IsRequired();
        builder.Property(a => a.CABId).HasColumnType("varchar(36)").IsRequired();
        builder.Property(a => a.SubStatus).HasColumnType("text").IsRequired();
        builder.Property(a => a.CreatedByUserGroup).HasColumnType("text").IsRequired();
        builder.Property(a => a.URLSlug).HasColumnType("text").IsRequired();
        builder.Property(a => a.Name).HasColumnType("text").IsRequired(false);
        builder.Property(a => a.UKASReference).HasColumnType("text").IsRequired(false);
        builder.Property(a => a.CABNumber).HasColumnType("text").IsRequired(false);
        builder.Property(e => e.CabBlob)
               .HasColumnType("jsonb")
               .HasConversion(
                   v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                   v => JsonSerializer.Deserialize<Document>(v, (JsonSerializerOptions)null));
        builder.Property(a => a.Version).HasColumnType("text").IsRequired();
    }
}