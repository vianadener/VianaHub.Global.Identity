using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using VianaHub.Global.Identity.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace VianaHub.Global.Identity.Infra.Data.Repository;

public class TenantDataRepository : ITenantDataRepository
{
    private readonly IdentityDbContext _context;

    public TenantDataRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<TenantEntity> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _context.Set<TenantEntity>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
    }

    public async Task<TenantEntity> GetByUserNameAsync(string name, CancellationToken ct)
    {
        await _context.Database.OpenConnectionAsync(ct);
        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_set_session_context @key=N'IsSuperAdmin', @value=1", ct);

        var result = await _context.Set<TenantEntity>()
            .AsNoTracking()
            .Include(x => x.Users)
            .Where(x => !x.IsDeleted && x.Users.Any(u => u.Name == name))
            .FirstOrDefaultAsync(ct);

        await _context.Database.CloseConnectionAsync();

        return result;
    }

    public async Task<TenantEntity> GetByLoginIdentifierAsync(string loginIdentifier, CancellationToken ct)
    {
        await _context.Database.OpenConnectionAsync(ct);
        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_set_session_context @key=N'IsSuperAdmin', @value=1", ct);

        var result = await _context.Tenants
            .AsNoTracking()
            .Include(x => x.Users)
            .Where(x => !x.IsDeleted && x.Users.Any(u => u.NormalizedLoginIdentifier == loginIdentifier))
            .FirstOrDefaultAsync(ct);

        await _context.Database.CloseConnectionAsync();

        return result;
    }

    public async Task<IEnumerable<TenantEntity>> GetLoginAsync(CancellationToken ct)
    {
        return await _context.Set<TenantEntity>()
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<TenantEntity>> GetAllAsync(CancellationToken ct)
    {
        return await _context.Set<TenantEntity>()
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            // order by Id instead of Name to avoid SQL errors when 'Name' column is missing in some DB schemas
            .OrderBy(x => x.Id)
            .ToListAsync(ct);
    }

    public async Task<ListPage<TenantEntity>> GetPagedAsync(PagedFilter request, CancellationToken ct)
    {
        var query = _context.Set<TenantEntity>().AsNoTracking().Where(x => !x.IsDeleted);

        // ?? Filtro de busca
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();

            query = query.Where(x =>
                EF.Functions.Like(x.Name.ToLower(), $"%{search}%") ||
                EF.Functions.Like(x.Alias.ToLower(), $"%{search}%")
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

        return new ListPage<TenantEntity>
        {
            Items = result,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = count,
            TotalPages = (int)Math.Ceiling((double)count / pageSize)
        };
    }

    public async Task<bool> ExistsByIdAsync(int id, CancellationToken ct)
    {
        return await _context.Set<TenantEntity>().AsNoTracking().AnyAsync(x => x.Id == id && !x.IsDeleted, ct);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct)
    {
        return await _context.Set<TenantEntity>().AsNoTracking().AnyAsync(x => x.Name == name && !x.IsDeleted, ct);
    }

    public async Task<bool> CreateAsync(TenantEntity entity, CancellationToken ct)
    {
        await _context.Tenants.AddAsync(entity, ct);
        return await _context.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> UpdateAsync(TenantEntity entity, CancellationToken ct)
    {
        _context.Tenants.Update(entity);
        return await _context.SaveChangesAsync(ct) > 0;
    }
}
