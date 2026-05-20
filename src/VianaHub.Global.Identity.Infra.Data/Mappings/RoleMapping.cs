using VianaHub.Global.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VianaHub.Global.Identity.Infra.Data.Mappings;

/// <summary>
/// Mapeamento da entidade Role
/// Roles por tenant e aplicação com suporte a Row Level Security
/// </summary>
public class RoleMapping : IEntityTypeConfiguration<RoleEntity>
{
    public void Configure(EntityTypeBuilder<RoleEntity> builder)
    {
        builder.ToTable("Roles", "dbo");

        // Chave Primária
        builder.HasKey(x => x.Id)
            .HasName("PK_Roles");

        builder.Property(x => x.Id)
            .UseIdentityColumn(1, 1)
            .IsRequired();

        // Propriedades
        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.AppId)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnType("NVARCHAR(100)")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnType("NVARCHAR(500)")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false)
            .IsRequired();

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

        // Constraints únicos
        builder.HasIndex(x => new { x.TenantId })
            .IsUnique()
            .HasDatabaseName("UQ_Roles_Id_Tenant");

        builder.HasIndex(x => new { x.TenantId, x.AppId, x.Name })
            .IsUnique()
            .HasDatabaseName("UQ_Roles_Tenant_AppId_Name");

        builder.HasIndex(x => new { x.TenantId, x.AppId, x.Id })
            .IsUnique()
            .HasDatabaseName("UQ_Roles_Tenant_AppId_Id");

        // Relacionamentos
        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("FK_Roles_Tenant")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.App)
            .WithMany()
            .HasForeignKey("TenantId", "AppId")
            .HasConstraintName("FK_Roles_Apps")
            .HasPrincipalKey("TenantId", "Id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Permissions)
            .WithOne(rp => rp.Role)
            .HasForeignKey("TenantId", "AppId", "RoleId")
            .HasConstraintName("FK_RolePermissions_Role")
            .HasPrincipalKey("TenantId", "AppId", "Id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.UserRoles)
            .WithOne(ur => ur.Role)
            .HasForeignKey("TenantId", "AppId", "RoleId")
            .HasConstraintName("FK_UserRoles_Role")
            .HasPrincipalKey("TenantId", "AppId", "Id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
