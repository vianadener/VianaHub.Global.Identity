using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using VianaHub.Global.Identity.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace VianaHub.Global.Identity.Infra.Data.Repository;

public class RolePermissionDataRepository : IRolePermissionDataRepository
{
    private readonly IdentityDbContext _context;

    public RolePermissionDataRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<IList<RolePermissionEntity>> GetAllAsync(int tenantId, int appId, CancellationToken ct)
    {
        return await _context.RolePermissions
            .AsNoTracking()
            .Include(x => x.Role)
            .Include(x => x.Resource)
            .Include(x => x.Action)
            .Where(x => x.TenantId == tenantId && x.AppId == appId)
            .ToListAsync(ct);
    }
    public async Task<RolePermissionEntity> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _context.RolePermissions
            .AsNoTracking()
            .Include(x => x.Tenant)
            .Include(x => x.Role)
            .Include(x => x.Resource)
            .Include(x => x.Action)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<RolePermissionEntity> GetByRoleIdAsync(int roleId, CancellationToken ct)
    {
        return await _context.RolePermissions
            .AsNoTracking()
            .Include(x => x.Tenant)
            .Include(x => x.Role)
            .Include(x => x.Resource)
            .Include(x => x.Action)
            .FirstOrDefaultAsync(x => x.RoleId == roleId, ct);
    }

    public async Task<IList<RolePermissionEntity>> GetByRoleAsync(int roleId, int tenantId, CancellationToken ct)
    {
        return await _context.RolePermissions
            .AsNoTracking()
            .Include(x => x.Role)
            .Include(x => x.Resource)
            .Include(x => x.Action)
            .Where(x => x.RoleId == roleId && x.TenantId == tenantId)
            .ToListAsync(ct);
    }


    public async Task<ListPage<RolePermissionEntity>> GetPagedAsync(int tenantId, int appId, PagedFilter request, CancellationToken ct)
    {
        var query = _context.Set<RolePermissionEntity>()
            .AsNoTracking()
            .Include(x => x.Tenant)
            .Include(x => x.App)
            .Include(x => x.Role)
            .Include(x => x.Resource)
            .Include(x => x.Action)
            .Where(x => x.TenantId == tenantId && x.AppId == appId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();

            query = query.Where(x =>
                EF.Functions.Like(x.App.Name.ToLower(), $"%{search}%") ||
                EF.Functions.Like(x.Role.Name.ToLower(), $"%{search}%") ||
                EF.Functions.Like(x.Resource.Name.ToLower(), $"%{search}%") ||
                EF.Functions.Like(x.Action.Name.ToLower(), $"%{search}%") ||
                EF.Functions.Like(x.App.Description.ToLower(), $"%{search}%") ||
                EF.Functions.Like(x.Role.Description.ToLower(), $"%{search}%") ||
                EF.Functions.Like(x.Resource.Description.ToLower(), $"%{search}%") ||
                EF.Functions.Like(x.Action.Description.ToLower(), $"%{search}%")
            );
        }

        var count = await query.CountAsync(ct);

        var orderedQuery = CreateSort.ApplyOrdering(query, request);

        var pageNumber = request.PageNumber ?? 1;
        var pageSize = request.PageSize ?? Paging.MinPageSize();

        var result = await orderedQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new ListPage<RolePermissionEntity>
        {
            Items = result,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = count,
            TotalPages = (int)Math.Ceiling((double)count / pageSize)
        };
    }
    public async Task<bool> ExistsAsync(int tenantId, int appId, int roleId, int resourceId, int actionId, CancellationToken ct)
    {
        return await _context.RolePermissions
            .AsNoTracking()
            .AnyAsync(x => x.TenantId == tenantId && x.AppId == appId && x.RoleId == roleId && x.ResourceId == resourceId && x.ActionId == actionId, ct);
    }

    public async Task<bool> CreateAsync(RolePermissionEntity entity, CancellationToken ct)
    {
        await _context.RolePermissions.AddAsync(entity, ct);
        return await _context.SaveChangesAsync(ct) > 0;
    }
    public async Task<bool> DeleteAsync(RolePermissionEntity entity, CancellationToken ct)
    {
        _context.RolePermissions.Remove(entity);
        return await _context.SaveChangesAsync(ct) > 0;
    }
}
