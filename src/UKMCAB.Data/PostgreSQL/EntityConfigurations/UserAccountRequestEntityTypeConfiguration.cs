using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Data.PostgreSQL.EntityConfigurations;

public class UserAccountRequestEntityTypeConfiguration : IEntityTypeConfiguration<UserAccountRequest>
{
    public void Configure(EntityTypeBuilder<UserAccountRequest> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).HasColumnType("uuid").IsRequired();

        builder.Property(e => e.AuditLog)
               .HasColumnType("jsonb")
               .HasConversion(
                   v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                   v => JsonSerializer.Deserialize<List<Audit>>(v, (JsonSerializerOptions)null));
        builder.Property(a => a.SubjectId).HasColumnType("text").IsRequired(false);
        builder.Property(a => a.FirstName).HasColumnType("text").IsRequired(false);
        builder.Property(a => a.Surname).HasColumnType("text").IsRequired(false);
        builder.Property(a => a.OrganisationName).HasColumnType("text").IsRequired(false);
        builder.Property(a => a.EmailAddress).HasColumnType("text").IsRequired(false);
        builder.Property(a => a.ContactEmailAddress).HasColumnType("text").IsRequired(false);
        builder.Property(a => a.Comments).HasColumnType("text").IsRequired(false);
        builder.Property(a => a.Status).HasColumnType("text").IsRequired(false);
        builder.Property(a => a.ReviewComments).HasColumnType("text").IsRequired(false);
        builder.Property(a => a.CreatedUtc).HasColumnType("timestamptz").IsRequired(false);
        builder.Property(a => a.LastUpdatedUtc).HasColumnType("timestamptz").IsRequired(false);
    }
}
