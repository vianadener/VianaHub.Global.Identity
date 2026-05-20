using VianaHub.Global.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VianaHub.Global.Identity.Infra.Data.Mappings;

/// <summary>
/// Mapeamento da entidade Resource
/// Recursos do sistema por tenant e aplicação
/// </summary>
public class ResourceMapping : IEntityTypeConfiguration<ResourceEntity>
{
    public void Configure(EntityTypeBuilder<ResourceEntity> builder)
    {
        builder.ToTable("Resources", "dbo");

        // Chave Primária
        builder.HasKey(r => r.Id)
            .HasName("PK_Resources");

        builder.Property(r => r.Id)
            .UseIdentityColumn(1, 1)
            .IsRequired();

        // Propriedades
        builder.Property(r => r.TenantId)
            .IsRequired();

        builder.Property(r => r.AppId)
            .IsRequired();

        builder.Property(r => r.Name)
            .HasColumnType("NVARCHAR(200)")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.Description)
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
        builder.HasIndex(r => new { r.TenantId, r.AppId, r.Name })
            .IsUnique()
            .HasDatabaseName("UQ_Resources_Tenant_AppId_Name");

        builder.HasIndex(r => new { r.TenantId, r.AppId, r.Id })
            .IsUnique()
            .HasDatabaseName("UQ_Resources_Tenant_AppId_Id");

        // Relacionamentos
        builder.HasOne(r => r.Tenant)
            .WithMany()
            .HasForeignKey(r => r.TenantId)
            .HasConstraintName("FK_Resources_Tenant")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.App)
            .WithMany()
            .HasForeignKey("TenantId", "AppId")
            .HasConstraintName("FK_Resources_Apps")
            .HasPrincipalKey("TenantId", "Id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.RolePermissions)
            .WithOne(rp => rp.Resource)
            .HasForeignKey("TenantId", "AppId", "ResourceId")
            .HasConstraintName("FK_RolePermissions_Resource")
            .HasPrincipalKey("TenantId", "AppId", "Id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
