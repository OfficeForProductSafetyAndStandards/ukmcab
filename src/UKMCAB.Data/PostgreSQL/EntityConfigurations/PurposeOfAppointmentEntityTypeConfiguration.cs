using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKMCAB.Data.Models.LegislativeAreas;

namespace UKMCAB.Data.PostgreSQL.EntityConfigurations;

public class PurposeOfAppointmentEntityTypeConfiguration : IEntityTypeConfiguration<PurposeOfAppointment>
{
    public void Configure(EntityTypeBuilder<PurposeOfAppointment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).HasColumnType("uuid").IsRequired();
        builder.Property(a => a.Name).HasColumnType("text").IsRequired();
        builder.Property(a => a.LegislativeAreaId).HasColumnType("uuid").IsRequired();
    }
}