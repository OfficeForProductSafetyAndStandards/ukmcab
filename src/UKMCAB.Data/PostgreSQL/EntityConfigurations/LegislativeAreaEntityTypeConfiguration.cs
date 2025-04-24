
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKMCAB.Data.Models.LegislativeAreas;

namespace UKMCAB.Data.PostgreSQL.EntityConfigurations;

public class LegislativeAreaEntityTypeConfiguration : IEntityTypeConfiguration<LegislativeArea>
{
    public void Configure(EntityTypeBuilder<LegislativeArea> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).HasColumnType("uuid").IsRequired();
        builder.Property(a => a.Name).HasColumnType("varchar(1024)").IsRequired();
        builder.Property(a => a.Regulation).HasColumnType("varchar(1024)").IsRequired();
        builder.Property(a => a.HasDataModel).HasColumnType("bool").IsRequired();
        builder.Property(a => a.RoleId).HasColumnType("varchar(1024)").IsRequired();
    }
}
