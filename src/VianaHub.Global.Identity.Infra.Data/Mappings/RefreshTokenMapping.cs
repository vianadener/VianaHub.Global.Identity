using VianaHub.Global.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VianaHub.Global.Identity.Infra.Data.Mappings;

/// <summary>
/// Mapeamento da entidade RefreshToken
/// Tokens de atualização para renovação de access tokens com suporte a Row Level Security
/// </summary>
public class RefreshTokenMapping : IEntityTypeConfiguration<RefreshTokenEntity>
{
    public void Configure(EntityTypeBuilder<RefreshTokenEntity> builder)
    {
        builder.ToTable("RefreshTokens", "dbo");

        // Chave Primária
        builder.HasKey(x => x.Id)
            .HasName("PK_RefreshTokens");

        builder.Property(x => x.Id)
            .UseIdentityColumn(1, 1)
            .IsRequired();

        // Propriedades
        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.AppId)
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

        builder.Property(x => x.RevokedAt)
            .HasColumnType("DATETIME2(7)")
            .IsRequired(false);

        builder.Property(x => x.RevokedBy)
            .HasColumnType("INT")
            .IsRequired(false);

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

        // Índices
        builder.HasIndex(x => new { x.TenantId, x.UserId })
            .HasDatabaseName("IX_RefreshTokens_User_Active")
            .HasFilter("RevokedAt IS NULL");

        builder.HasIndex(x => new { x.TenantId, x.ExpiresAt })
            .HasDatabaseName("IX_RefreshTokens_ExpiresAt")
            .HasFilter("RevokedAt IS NULL");

        // Índice único para TokenHash por Tenant e App
        builder.HasIndex(x => new { x.TenantId, x.AppId, x.TokenHash })
            .IsUnique()
            .HasDatabaseName("UQ_RefreshTokens");

        // Relacionamentos
        // App -> RefreshTokens
        // A FK (TenantId, AppId) referencia (TenantId, Id) da tabela Apps
        builder.HasOne<AppEntity>()
            .WithMany()
            .HasForeignKey(x => new { x.TenantId, x.AppId })
            .HasPrincipalKey(a => new { a.TenantId, a.Id })
            .HasConstraintName("FK_RefreshTokens_Apps")
            .OnDelete(DeleteBehavior.Restrict);

        // User -> RefreshTokens
        // A FK (UserId, TenantId) referencia (Id, TenantId) da tabela Users
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => new { x.UserId, x.TenantId })
            .HasPrincipalKey(u => new { u.Id, u.TenantId })
            .HasConstraintName("FK_RefreshTokens_User")
            .OnDelete(DeleteBehavior.Restrict);

        // RevokedBy User
        // A FK (RevokedBy, TenantId) referencia (Id, TenantId) da tabela Users
        builder.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(x => new { x.RevokedBy, x.TenantId })
            .HasPrincipalKey(u => new { u.Id, u.TenantId })
            .HasConstraintName("FK_RefreshTokens_RevokedBy")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Tenant -> RefreshTokens já configurado no TenantMapping
    }
}
