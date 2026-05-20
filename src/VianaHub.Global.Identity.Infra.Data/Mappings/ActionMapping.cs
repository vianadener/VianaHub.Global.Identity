using VianaHub.Global.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VianaHub.Global.Identity.Infra.Data.Mappings;

/// <summary>
/// Mapeamento da entidade ActionEntity
/// Ações possíveis no sistema por tenant e aplicação
/// </summary>
public class ActionMapping : IEntityTypeConfiguration<ActionEntity>
{
    public void Configure(EntityTypeBuilder<ActionEntity> builder)
    {
        builder.ToTable("Actions", "dbo");

        // Chave Primária
        builder.HasKey(x => x.Id)
            .HasName("PK_Actions");

        builder.Property(x => x.Id)
            .UseIdentityColumn(1, 1)
            .IsRequired();

        // Propriedades
        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.AppId)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnType("NVARCHAR(50)")
            .HasMaxLength(50)
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
        builder.HasIndex(x => new { x.TenantId, x.AppId, x.Name })
            .IsUnique()
            .HasDatabaseName("UQ_Actions_Tenant_AppId_Name");

        builder.HasIndex(x => new { x.TenantId, x.AppId, x.Id })
            .IsUnique()
            .HasDatabaseName("UQ_Actions_Tenant_AppsId_Id");

        // Relacionamentos
        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("FK_Actions_Tenant")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.App)
            .WithMany()
            .HasForeignKey("TenantId", "AppId")
            .HasConstraintName("FK_Actions_Apps")
            .HasPrincipalKey("TenantId", "Id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.RolePermissions)
            .WithOne(rp => rp.Action)
            .HasForeignKey("TenantId", "AppId", "ActionId")
            .HasConstraintName("FK_RolePermissions_Action")
            .HasPrincipalKey("TenantId", "AppId", "Id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
