using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKMCAB.Data.Models.LegislativeAreas;

namespace UKMCAB.Data.PostgreSQL.EntityConfigurations;

public class AreaOfCompetencyEntityTypeConfiguration : IEntityTypeConfiguration<AreaOfCompetency>
{
    public void Configure(EntityTypeBuilder<AreaOfCompetency> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).HasColumnType("uuid").IsRequired();
        builder.Property(a => a.Name).HasColumnType("text").IsRequired();
        builder.Property(a => a.ProtectionAgainstRiskIds).HasColumnType("uuid[]").IsRequired();
    }
}
