using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using VianaHub.Global.Identity.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace VianaHub.Global.Identity.Infra.Data.Repository;

public class ActionDataRepository : IActionDataRepository
{
    private readonly IdentityDbContext _context;

    public ActionDataRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ActionEntity>> GetAllAsync(int tenantId, int appId, CancellationToken ct)
    {
        return await _context.Set<ActionEntity>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AppId == appId && !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }
    public async Task<ActionEntity> GetByIdAsync(int tenantId, int appId, int id, CancellationToken ct)
    {
        return await _context.Set<ActionEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.AppId == appId && x.Id == id && !x.IsDeleted, ct);
    }
    public async Task<ListPage<ActionEntity>> GetPagedAsync(int tenantId, int appId, PagedFilter request, CancellationToken ct)
    {
        var query = _context.Set<ActionEntity>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AppId == appId && !x.IsDeleted);

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

        return new ListPage<ActionEntity>
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
        return await _context.Set<ActionEntity>()
            .AsNoTracking()
            .AnyAsync(x => x.TenantId == tenantId && x.AppId == appId && x.Name == name && !x.IsDeleted, ct);
    }

    public async Task<bool> CreateAsync(ActionEntity entity, CancellationToken ct)
    {
        await _context.Set<ActionEntity>().AddAsync(entity, ct);
        return await _context.SaveChangesAsync(ct) > 0;
    }
    public async Task<bool> UpdateAsync(ActionEntity entity, CancellationToken ct)
    {
        _context.Set<ActionEntity>().Update(entity);
        return await _context.SaveChangesAsync(ct) > 0;
    }
}
