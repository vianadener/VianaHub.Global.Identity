using VianaHub.Global.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VianaHub.Global.Identity.Infra.Data.Mappings;

/// <summary>
/// Mapeamento da entidade PasswordResetToken
/// Tokens one-time use para recuperação de senha com TTL de 15 minutos
/// </summary>
public class PasswordResetTokenMapping : IEntityTypeConfiguration<PasswordResetTokenEntity>
{
    public void Configure(EntityTypeBuilder<PasswordResetTokenEntity> builder)
    {
        builder.ToTable("PasswordResetTokens", "dbo");

        builder.HasKey(x => x.Id)
            .HasName("PK_PasswordResetTokens");

        builder.Property(x => x.Id)
            .UseIdentityColumn(1, 1)
            .IsRequired();

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.TokenHash)
            .HasColumnName("TokenHash")
            .HasColumnType("VARBINARY(64)")
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .HasColumnType("DATETIME2(7)")
            .IsRequired();

        builder.Property(x => x.Used)
            .HasColumnType("BIT")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.AddedBy)
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

        builder.HasIndex(x => new { x.TenantId, x.TokenHash })
            .HasDatabaseName("IX_PasswordResetTokens_TenantId_TokenHash")
            .IsUnique();

        builder.HasIndex(x => new { x.TenantId, x.UserId, x.AddedOn })
            .HasDatabaseName("IX_PasswordResetTokens_RateLimit");

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => new { x.UserId, x.TenantId })
            .HasPrincipalKey(x => new { x.Id, x.TenantId })
            .HasConstraintName("FK_PasswordResetTokens_User")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
