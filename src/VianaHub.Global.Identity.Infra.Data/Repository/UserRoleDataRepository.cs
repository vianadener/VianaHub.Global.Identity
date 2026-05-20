using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using VianaHub.Global.Identity.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace VianaHub.Global.Identity.Infra.Data.Repository;

public class UserRoleDataRepository : IUserRoleDataRepository
{
    private readonly IdentityDbContext _context;

    public UserRoleDataRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<IList<UserRoleEntity>> GetAllAsync(int tenantId, int appId, CancellationToken ct)
    {
        return await _context.UserRoles
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.Tenant)
            .Include(x => x.App)
            .Include(x => x.User)
            .Include(x => x.Role)
            .Where(x => x.TenantId == tenantId && x.AppId == appId)
            .ToListAsync(ct);
    }
    public async Task<UserRoleEntity> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _context.UserRoles
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.Tenant)
            .Include(x => x.App)
            .Include(x => x.User)
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<IList<UserRoleEntity>> GetByUserIdAsync(int userId, CancellationToken ct)
    {
        return await _context.UserRoles
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.Tenant)
            .Include(x => x.App)
            .Include(x => x.User)
            .Include(x => x.Role)
            .Where(x => x.User.Id == userId)
            .ToListAsync(ct);
    }

    
    public async Task<ListPage<UserRoleEntity>> GetPagedAsync(int tenantId, int appId, PagedFilter request, CancellationToken ct)
    {
        var query = _context.Set<UserRoleEntity>()
            .AsNoTracking()
            .Include (x => x.Tenant)
            .Include(x => x.App)
            .Include(x => x.User)
            .Include(x => x.Role)
            .Where(x => x.TenantId == tenantId && x.AppId == appId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();

            query = query.Where(x =>
                EF.Functions.Like(x.App.Name.ToLower(), $"%{search}%") || 
                EF.Functions.Like(x.User.Name.ToLower(), $"%{search}%") ||
                EF.Functions.Like(x.Role.Name.ToLower(), $"%{search}%") ||
                EF.Functions.Like(x.App.Description.ToLower(), $"%{search}%") ||
                EF.Functions.Like(x.Role.Description.ToLower(), $"%{search}%")
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

        return new ListPage<UserRoleEntity>
        {
            Items = result,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = count,
            TotalPages = (int)Math.Ceiling((double)count / pageSize)
        };
    }
    public async Task<bool> ExistsAsync(int tenantId, int appId, int userId, int roleId, CancellationToken ct)
    {
        return await _context.UserRoles.AnyAsync(x => x.TenantId == tenantId && x.AppId == appId && x.UserId == userId && x.RoleId == roleId, ct);
    }

    public async Task<bool> CreateAsync(UserRoleEntity entity, CancellationToken ct)
    {
        await _context.UserRoles.AddAsync(entity, ct);
        return await _context.SaveChangesAsync(ct) > 0;
    }
    public async Task<bool> DeleteAsync(UserRoleEntity entity,CancellationToken ct)
    {
        _context.UserRoles.Remove(entity);
        return await _context.SaveChangesAsync(ct) > 0;
    }
}
