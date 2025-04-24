
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKMCAB.Data.Models.LegislativeAreas;

namespace UKMCAB.Data.PostgreSQL.EntityConfigurations;

public class ProtectionAgainstRiskEntityTypeConfiguration : IEntityTypeConfiguration<ProtectionAgainstRisk>
{
    public void Configure(EntityTypeBuilder<ProtectionAgainstRisk> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).HasColumnType("uuid").IsRequired();
        builder.Property(a => a.Name).HasColumnType("varchar(1024)").IsRequired();
        builder.Property(a => a.PpeProductTypeIds).HasColumnType("uuid[]").IsRequired();
    }
}
