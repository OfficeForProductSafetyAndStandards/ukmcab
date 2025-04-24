
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKMCAB.Data.Models.LegislativeAreas;

namespace UKMCAB.Data.PostgreSQL.EntityConfigurations;

public class ProcedureEntityTypeConfiguration : IEntityTypeConfiguration<Procedure>
{
    public void Configure(EntityTypeBuilder<Procedure> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).HasColumnType("uuid").IsRequired();
        builder.Property(a => a.Name).HasColumnType("varchar(1024)").IsRequired();
        builder.Property(a => a.LegislativeAreaId).HasColumnType("uuid").IsRequired();
        builder.Property(a => a.PurposeOfAppointmentIds).HasColumnType("uuid[]").IsRequired();
        builder.Property(a => a.CategoryIds).HasColumnType("uuid[]").IsRequired();
        builder.Property(a => a.ProductIds).HasColumnType("uuid[]").IsRequired();
        builder.Property(a => a.PpeProductTypeIds).HasColumnType("uuid[]").IsRequired();
        builder.Property(a => a.ProtectionAgainstRiskIds).HasColumnType("uuid[]").IsRequired();
        builder.Property(a => a.AreaOfCompetencyIds).HasColumnType("uuid[]").IsRequired();
    }
}
