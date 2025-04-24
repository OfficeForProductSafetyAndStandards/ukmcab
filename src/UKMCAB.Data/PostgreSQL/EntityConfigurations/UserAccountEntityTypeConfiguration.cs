using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Data.PostgreSQL.EntityConfigurations;

public class UserAccountEntityTypeConfiguration : IEntityTypeConfiguration<UserAccount>
{
    public void Configure(EntityTypeBuilder<UserAccount> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).HasColumnType("varchar(1000)").IsRequired();

        builder.Property(e => e.AuditLog)
               .HasColumnType("jsonb")
               .HasConversion(
                   v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                   v => JsonSerializer.Deserialize<List<Audit>>(v, (JsonSerializerOptions)null));
        builder.Property(a => a.FirstName).HasColumnType("text").IsRequired(false);
        builder.Property(a => a.Surname).HasColumnType("text").IsRequired(false);
        builder.Property(a => a.SurnameNormalized).HasColumnType("text").IsRequired(false);
        builder.Property(a => a.OrganisationName).HasColumnType("text").IsRequired(false);
        builder.Property(a => a.EmailAddress).HasColumnType("text").IsRequired(false);
        builder.Property(a => a.ContactEmailAddress).HasColumnType("text").IsRequired(false);
        builder.Property(a => a.PasswordHash).HasColumnType("text").IsRequired(false);
        builder.Property(a => a.IsLocked).HasColumnType("bool").IsRequired();
        builder.Property(a => a.LockReason).HasColumnType("text").IsRequired(false);
        builder.Property(a => a.CreatedUtc).HasColumnType("timestamptz").IsRequired();
        builder.Property(a => a.LastUpdatedUtc).HasColumnType("timestamptz").IsRequired();
        builder.Property(a => a.LastLogonUtc).HasColumnType("timestamptz").IsRequired(false);
        builder.Property(a => a.LockReasonDescription).HasColumnType("text").IsRequired(false);
        builder.Property(a => a.LockInternalNotes).HasColumnType("text").IsRequired(false);
        builder.Property(a => a.Role).HasColumnType("text").IsRequired(false);
    }
}
