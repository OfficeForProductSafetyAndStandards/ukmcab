
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKMCAB.Data.Models.LegislativeAreas;

namespace UKMCAB.Data.PostgreSQL.EntityConfigurations;

public class ProductEntityTypeConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).HasColumnType("uuid").IsRequired();
        builder.Property(a => a.Name).HasColumnType("varchar(1024)").IsRequired();
        builder.Property(a => a.LegislativeAreaId).HasColumnType("uuid").IsRequired(false);
        builder.Property(a => a.PurposeOfAppointmentId).HasColumnType("uuid").IsRequired(false);
        builder.Property(a => a.CategoryId).HasColumnType("uuid").IsRequired(false);
        builder.Property(a => a.SubCategoryId).HasColumnType("uuid").IsRequired(false);
    }
}
