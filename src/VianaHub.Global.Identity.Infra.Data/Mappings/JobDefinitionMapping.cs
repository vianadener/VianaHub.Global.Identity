using VianaHub.Global.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VianaHub.Global.Identity.Infra.Data.Mappings;

public class JobDefinitionMapping : IEntityTypeConfiguration<JobDefinitionEntity>
{
    public void Configure(EntityTypeBuilder<JobDefinitionEntity> builder)
    {
        // Map to dbo schema - the database currently uses dbo.JobDefinitions
        builder.ToTable("JobDefinitions", "dbo");

        // Primary key and identity
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .UseIdentityColumn(1, 1)
            .IsRequired();

        builder.Property(x => x.HangfireJobId)
            .HasColumnType("NVARCHAR(100)")
            .HasMaxLength(100);

        builder.Property(x => x.Category)
            .HasColumnType("NVARCHAR(100)")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasColumnType("NVARCHAR(200)")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnType("NVARCHAR(200)")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnType("NVARCHAR(1000)")
            .HasMaxLength(1000);

        builder.Property(x => x.Purpose)
            .HasColumnType("NVARCHAR(1000)")
            .HasMaxLength(1000);

        builder.Property(x => x.CronExpression)
            .HasColumnType("NVARCHAR(200)")
            .HasMaxLength(200);

        builder.Property(x => x.Configuration)
            .HasColumnType("NVARCHAR(MAX)");

        builder.Property(x => x.Method)
            .HasColumnType("NVARCHAR(200)")
            .HasMaxLength(200);

        builder.Property(x => x.TimeZoneId)
            .HasColumnType("NVARCHAR(200)")
            .HasMaxLength(200);

        builder.Property(x => x.ExecuteOnlyOnce)
            .HasColumnType("BIT")
            .IsRequired();

        builder.Property(x => x.TimeoutMinutes)
            .HasColumnType("INT")
            .IsRequired();

        builder.Property(x => x.Priority)
            .HasColumnType("INT")
            .IsRequired();

        builder.Property(x => x.Queue)
            .HasColumnType("NVARCHAR(200)")
            .HasMaxLength(200);

        builder.Property(x => x.MaxRetries)
            .HasColumnType("INT")
            .IsRequired();

        builder.Property(x => x.IsSystemJob)
            .HasColumnType("BIT")
            .IsRequired();

        builder.Property(x => x.LastRegisteredAt)
            .HasColumnType("DATETIME2(7)");

        builder.Property(x => x.IsActive)
            .HasColumnType("BIT")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .HasColumnType("BIT")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.AddedBy)
            .HasColumnType("INT")
            .IsRequired();

        builder.Property(x => x.AddedOn)
            .HasColumnType("DATETIME2(7)")
            .IsRequired();

        builder.Property(x => x.ModifiedBy)
            .HasColumnType("INT")
            .IsRequired(false);

        builder.Property(x => x.ModifiedAt)
            .HasColumnType("DATETIME2(7)")
            .IsRequired(false);

        // Indexes
        builder.HasIndex(x => new { x.Category, x.IsActive, x.IsDeleted }).HasDatabaseName("IX_Services_Category_Active");
        builder.HasIndex(x => new { x.IsActive, x.IsSystemJob }).HasFilter("IsDeleted = 0").HasDatabaseName("IX_Services_Active_SYSTEM");
        builder.HasIndex(x => x.HangfireJobId).HasFilter("HangfireJobId IS NOT NULL").HasDatabaseName("IX_Services_HangfireJobId");

        // Unique constraint
        builder.HasIndex(x => x.Name).IsUnique().HasDatabaseName("UQ_Job_JobName");

    }
}
