using VianaHub.Global.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VianaHub.Global.Identity.Infra.Data.Mappings;

/// <summary>
/// Mapeamento da entidade User
/// Usuários do sistema com suporte a Row Level Security
/// </summary>
public class UserMapping : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("Users", "dbo");

        // Chave Primária
        builder.HasKey(x => x.Id)
            .HasName("PK_Users");

        builder.Property(x => x.Id)
            .UseIdentityColumn(1, 1)
            .IsRequired();

        // Propriedades
        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnType("NVARCHAR(150)")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.LoginIdentifier)
            .HasColumnType("NVARCHAR(500)")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.NormalizedLoginIdentifier)
            .HasColumnType("NVARCHAR(500)")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.PasswordHash)
            .HasColumnType("NVARCHAR(500)")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.LastAccessAt)
            .HasColumnType("DATETIME2")
            .IsRequired(false);

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
        // Adiciona alternate key (Id, TenantId) para compatibilidade com FKs compostas no banco
        builder.HasAlternateKey(x => new { x.Id, x.TenantId })
            .HasName("UQ_Users_Id_Tenant");

        builder.HasIndex(x => new { x.TenantId, x.NormalizedLoginIdentifier })
            .IsUnique()
            .HasDatabaseName("UQ_Users_Tenant_NormalizedLoginIdentifier");

        // Relacionamentos
        // NOTA: O relacionamento User -> Tenant já está configurado no TenantMapping.cs
        // através de HasMany(t => t.Users).WithOne(x => x.Tenant).HasForeignKey(x => x.TenantId)
        // Não configurar novamente aqui para evitar propriedades sombra (shadow properties)

        builder.HasMany(x => x.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .HasConstraintName("FK_UserRoles_User")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
