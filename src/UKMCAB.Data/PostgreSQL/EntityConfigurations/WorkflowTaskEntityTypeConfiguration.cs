using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UKMCAB.Data.Models.Workflow;

namespace UKMCAB.Data.PostgreSQL.EntityConfigurations;

public class WorkflowTaskEntityTypeConfiguration : IEntityTypeConfiguration<WorkflowTask>
{
    public void Configure(EntityTypeBuilder<WorkflowTask> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).HasColumnType("varchar(1024)").IsRequired();
        builder.Property(a => a.TaskType).HasColumnType("text").IsRequired();
        builder.Property(e => e.Submitter)
               .HasColumnType("jsonb")
               .HasConversion(
                   v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                   v => JsonSerializer.Deserialize<Models.Users.UserAccount>(v, (JsonSerializerOptions)null));
        builder.Property(a => a.ForRoleId).HasColumnType("text").IsRequired();
        builder.Property(e => e.Assignee)
               .HasColumnType("jsonb")
               .HasConversion(
                   v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                   v => JsonSerializer.Deserialize<Models.Users.UserAccount>(v, (JsonSerializerOptions)null));

        builder.Property(a => a.Assigned).HasColumnType("timestamptz").IsRequired(false);
        builder.Property(a => a.Reason).HasColumnType("text").IsRequired();
        builder.Property(a => a.SentOn).HasColumnType("timestamptz").IsRequired();
        builder.Property(e => e.LastUpdatedBy)
               .HasColumnType("jsonb")
               .HasConversion(
                   v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                   v => JsonSerializer.Deserialize<Models.Users.UserAccount>(v, (JsonSerializerOptions)null));
        builder.Property(a => a.LastUpdatedOn).HasColumnType("timestamptz").IsRequired();
        builder.Property(a => a.Approved).HasColumnType("bool").IsRequired(false);
        builder.Property(a => a.DeclineReason).HasColumnType("text").IsRequired(false);
        builder.Property(a => a.Completed).HasColumnType("bool").IsRequired();
        builder.Property(a => a.CabId).HasColumnType("uuid").IsRequired(false);
        builder.Property(a => a.DocumentLAId).HasColumnType("uuid").IsRequired(false);
    }
}
