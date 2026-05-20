using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using VianaHub.Global.Identity.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace VianaHub.Global.Identity.Infra.Data.Repository;

public class RoleDataRepository : IRoleDataRepository
{
    private readonly IdentityDbContext _context;

    public RoleDataRepository(IdentityDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<RoleEntity>> GetAllAsync(int tenantId, int appId, CancellationToken ct)
    {
        return await _context.Set<RoleEntity>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AppId == appId && !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }
    public async Task<RoleEntity> GetByIdAsync(int tenantId, int appId, int id, CancellationToken ct)
    {
        return await _context.Set<RoleEntity>().AsNoTracking().FirstOrDefaultAsync(x => x.TenantId == tenantId && x.AppId == appId && x.Id == id && !x.IsDeleted, ct);
    }
    public async Task<ListPage<RoleEntity>> GetPagedAsync(int tenantId, int appId, PagedFilter request, CancellationToken ct)
    {
        var query = _context.Set<RoleEntity>().AsNoTracking().Where(x => x.TenantId == tenantId && x.AppId == appId && !x.IsDeleted);

        // ?? Filtro de busca
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();

            query = query.Where(x =>
                EF.Functions.Like(x.Name.ToLower(), $"%{search}%")
                || EF.Functions.Like(x.Description.ToLower(), $"%{search}%")
            );
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        var count = await query.CountAsync(ct);

        var orderedQuery = CreateSort.ApplyOrdering(query, request);

        var pageNumber = request.PageNumber ?? 1;
        var pageSize = request.PageSize ?? Paging.MinPageSize();

        var result = await orderedQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new ListPage<RoleEntity>
        {
            Items = result,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = count,
            TotalPages = (int)Math.Ceiling((double)count / pageSize)
        };
    }
    public async Task<bool> ExistsByNameAsync(int tenantId, int appId, string name, CancellationToken ct)
    {
        return await _context.Set<RoleEntity>().AsNoTracking().AnyAsync(x => x.TenantId == tenantId && x.AppId == appId && x.Name == name && !x.IsDeleted, ct);
    }

    public async Task<bool> CreateAsync(RoleEntity entity, CancellationToken ct)
    {
        await _context.Roles.AddAsync(entity, ct);
        return await _context.SaveChangesAsync(ct) > 0;
    }
    public async Task<bool> UpdateAsync(RoleEntity entity, CancellationToken ct)
    {
        _context.Roles.Update(entity);
        return await _context.SaveChangesAsync(ct) > 0;
    }
}
