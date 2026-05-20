using VianaHub.Global.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VianaHub.Global.Identity.Infra.Data.Mappings;

/// <summary>
/// Mapeamento da entidade UserRole
/// Relação usuário x role com suporte a Row Level Security
/// </summary>
public class UserRoleMapping : IEntityTypeConfiguration<UserRoleEntity>
{
    public void Configure(EntityTypeBuilder<UserRoleEntity> builder)
    {
        builder.ToTable("UserRoles", "dbo");

        // Chave Primária
        builder.HasKey(x => x.Id)
            .HasName("PK_UserRoles");

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

        builder.Property(x => x.RoleId)
            .IsRequired();

        // Constraints únicos
        builder.HasIndex(x => new { x.TenantId, x.AppId, x.UserId, x.RoleId })
            .IsUnique()
            .HasDatabaseName("UQ_UserRoles");

        // Índices
        builder.HasIndex(x => new { x.TenantId, x.UserId, x.AppId })
            .HasDatabaseName("IX_UserRoles_User_App")
            .IncludeProperties(x => x.RoleId);

        // Relacionamentos
        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("FK_UserRoles_Tenant")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.App)
            .WithMany()
            .HasForeignKey("TenantId", "AppId")
            .HasConstraintName("FK_UserRoles_Apps")
            .HasPrincipalKey("TenantId", "Id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey("UserId", "TenantId")
            .HasConstraintName("FK_UserRoles_User")
            .HasPrincipalKey("Id", "TenantId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey("TenantId", "AppId", "RoleId")
            .HasConstraintName("FK_UserRoles_Role")
            .HasPrincipalKey("TenantId", "AppId", "Id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
