using VianaHub.Global.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VianaHub.Global.Identity.Infra.Data.Mappings;

/// <summary>
/// Mapeamento da entidade Tenant
/// Tabela principal de tenants com suporte a Row Level Security
/// </summary>
public class TenantMapping : IEntityTypeConfiguration<TenantEntity>
{
    public void Configure(EntityTypeBuilder<TenantEntity> builder)
    {
        builder.ToTable("Tenants", "dbo");

        // Chave Primária
        builder.HasKey(x => x.Id)
            .HasName("PK_Tenants");

        builder.Property(x => x.Id)
            .UseIdentityColumn(1, 1)
            .IsRequired();

        // Propriedades
        builder.Property(x => x.Name)
            .HasColumnType("NVARCHAR(200)")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnType("NVARCHAR(500)")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Alias)
            .HasColumnType("NVARCHAR(30)")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.UrlImage)
            .HasColumnType("NVARCHAR(500)")
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(x => x.Settings)
            .HasColumnType("NVARCHAR(MAX)")
            .IsRequired(false);

        builder.Property(x => x.Remarks)
            .HasColumnType("NVARCHAR(1000)")
            .HasMaxLength(1000)
            .IsRequired(false);

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
            .HasColumnType("DATETIME2")
            .HasDefaultValueSql("SYSDATETIME()")
            .IsRequired();

        builder.Property(x => x.ModifiedBy)
            .HasColumnType("INT")
            .IsRequired(false);

        builder.Property(x => x.ModifiedAt)
            .HasColumnType("DATETIME2")
            .IsRequired(false);

        // Relacionamentos
        builder.HasMany(x => x.Users)
            .WithOne(u => u.Tenant)
            .HasForeignKey(u => u.TenantId)
            .HasConstraintName("FK_Users_Tenant")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
