using VianaHub.Global.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace VianaHub.Global.Identity.Infra.Data.Context;

public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }

    #region DbSets - RBAC Structure
    public DbSet<TenantEntity> Tenants { get; set; }
    public DbSet<AppEntity> Apps { get; set; }
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<RoleEntity> Roles { get; set; }
    public DbSet<ResourceEntity> Resources { get; set; }
    public DbSet<ActionEntity> Actions { get; set; }
    public DbSet<RolePermissionEntity> RolePermissions { get; set; }
    public DbSet<UserRoleEntity> UserRoles { get; set; }
    public DbSet<JwtKeyEntity> JwtKeys { get; set; }
    public DbSet<RefreshTokenEntity> RefreshTokens { get; set; }
    public DbSet<PasswordResetTokenEntity> PasswordResetTokens { get; set; }
    public DbSet<JobDefinitionEntity> JobDefinitionEntities { get; set; }

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica todas as configurações de mapeamento do assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);

        // Configura o schema padrão
        modelBuilder.HasDefaultSchema("dbo");
    }
}
