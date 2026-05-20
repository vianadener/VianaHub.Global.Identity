using VianaHub.Global.Identity.Domain.Base;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace VianaHub.Global.Identity.Tests.Infra.Data.Repository;

internal static class RepositoryTestHelper
{
    internal static IdentityDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new IdentityDbContext(options);
    }

    internal static void SetId<T>(T entity, int id) where T : class
        => typeof(Entity).GetProperty("Id")!.SetValue(entity, id);

    internal static TenantEntity BuildTenant(int id = 1)
    {
        var tenant = new TenantEntity("Tenant Test", "Desc", "alias", null, null, null, 1);
        SetId(tenant, id);
        return tenant;
    }

    internal static AppEntity BuildApp(int id = 1, int tenantId = 1)
    {
        var entity = new AppEntity(tenantId, "App Test", "Desc", 1);
        SetId(entity, id);
        return entity;
    }

    internal static ActionEntity BuildAction(int id = 1, int tenantId = 1, int appId = 1, string name = "Action Test")
    {
        var entity = new ActionEntity(tenantId, appId, name, "Desc", 1);
        SetId(entity, id);
        return entity;
    }

    internal static ResourceEntity BuildResource(int id = 1, int tenantId = 1, int appId = 1, string name = "Resource Test")
    {
        var entity = new ResourceEntity(tenantId, appId, name, "Desc", 1);
        SetId(entity, id);
        return entity;
    }

    internal static RoleEntity BuildRole(int id = 1, int tenantId = 1, int appId = 1, string name = "Role Test")
    {
        var entity = new RoleEntity(tenantId, appId, name, "Desc", 1);
        SetId(entity, id);
        return entity;
    }

    internal static UserEntity BuildUser(int id = 1, int tenantId = 1, string login = "user@test.com")
    {
        var entity = new UserEntity(tenantId, "User Test", login, "hash123", null, 1);
        SetId(entity, id);
        return entity;
    }

    internal static JwtKeyEntity BuildJwtKey(int id = 1, int tenantId = 1)
    {
        var entity = new JwtKeyEntity(tenantId, "publicKey", "privateKeyEncrypted", 1);
        SetId(entity, id);
        return entity;
    }

    internal static RefreshTokenEntity BuildRefreshToken(int id = 1, int tenantId = 1, int appId = 1, int userId = 1)
    {
        var entity = new RefreshTokenEntity(tenantId, appId, userId, new byte[] { 1, 2, 3 }, DateTime.UtcNow.AddHours(1), 1);
        SetId(entity, id);
        return entity;
    }

    internal static PasswordResetTokenEntity BuildPasswordResetToken(int id = 1, int tenantId = 1, int userId = 1)
    {
        var entity = new PasswordResetTokenEntity(tenantId, userId, new byte[] { 4, 5, 6 }, DateTime.UtcNow.AddMinutes(15), 1);
        SetId(entity, id);
        return entity;
    }

    internal static RolePermissionEntity BuildRolePermission(int id = 1, int tenantId = 1, int appId = 1, int roleId = 1, int resourceId = 1, int actionId = 1)
    {
        var entity = new RolePermissionEntity(tenantId, appId, roleId, resourceId, actionId);
        typeof(RolePermissionEntity).GetProperty("Id")!.SetValue(entity, id);
        return entity;
    }

    internal static UserRoleEntity BuildUserRole(int id = 1, int tenantId = 1, int appId = 1, int userId = 1, int roleId = 1)
    {
        var entity = new UserRoleEntity(tenantId, appId, userId, roleId);
        typeof(UserRoleEntity).GetProperty("Id")!.SetValue(entity, id);
        return entity;
    }

    internal static JobDefinitionEntity BuildJobDefinition(int id = 1, string name = "Job Test")
    {
        var entity = new JobDefinitionEntity("Category", name, "MyType", 1);
        SetId(entity, id);
        return entity;
    }
}
