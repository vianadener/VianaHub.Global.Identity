using VianaHub.Global.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VianaHub.Global.Identity.Infra.Data.Mappings;

/// <summary>
/// Mapeamento da entidade RolePermission
/// Permissões por role com suporte a Row Level Security
/// </summary>
public class RolePermissionMapping : IEntityTypeConfiguration<RolePermissionEntity>
{
    public void Configure(EntityTypeBuilder<RolePermissionEntity> builder)
    {
        builder.ToTable("RolePermissions", "dbo");

        // Chave Primária
        builder.HasKey(x => x.Id)
            .HasName("PK_RolePermissions");

        builder.Property(x => x.Id)
            .UseIdentityColumn(1, 1)
            .IsRequired();

        // Propriedades
        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.AppId)
            .IsRequired();

        builder.Property(x => x.RoleId)
            .IsRequired();

        builder.Property(x => x.ResourceId)
            .IsRequired();

        builder.Property(x => x.ActionId)
            .IsRequired();

        // Constraints únicos
        builder.HasIndex(x => new { x.TenantId, x.AppId, x.RoleId, x.ResourceId, x.ActionId })
            .IsUnique()
            .HasDatabaseName("UQ_RolePermissions");

        // Índice de lookup
        builder.HasIndex(x => new { x.TenantId, x.AppId, x.RoleId, x.ResourceId, x.ActionId })
            .HasDatabaseName("IX_RolePermissions_Lookup")
            .IncludeProperties(x => x.Id);

        // Relacionamentos
        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("FK_RolePermissions_Tenant")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.App)
            .WithMany()
            .HasForeignKey("TenantId", "AppId")
            .HasConstraintName("FK_RolePermissions_Apps")
            .HasPrincipalKey("TenantId", "Id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Role)
            .WithMany(r => r.Permissions)
            .HasForeignKey("TenantId", "AppId", "RoleId")
            .HasConstraintName("FK_RolePermissions_Role")
            .HasPrincipalKey("TenantId", "AppId", "Id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Resource)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey("TenantId", "AppId", "ResourceId")
            .HasConstraintName("FK_RolePermissions_Resource")
            .HasPrincipalKey("TenantId", "AppId", "Id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Action)
            .WithMany(a => a.RolePermissions)
            .HasForeignKey("TenantId", "AppId", "ActionId")
            .HasConstraintName("FK_RolePermissions_Action")
            .HasPrincipalKey("TenantId", "AppId", "Id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
