using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using VianaHub.Global.Identity.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace VianaHub.Global.Identity.Infra.Data.Repository;

public class JobDefinitionDataRepository : IJobDefinitionDataRepository
{
    private readonly IdentityDbContext _context;

    public JobDefinitionDataRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<JobDefinitionEntity>> GetAllAsync(CancellationToken ct)
    {
        return await _context.Set<JobDefinitionEntity>()
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Category)
            .ThenBy(x => x.Priority)
            .ThenBy(x => x.Name)
            .ToListAsync(ct);
    }
    public async Task<JobDefinitionEntity> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _context.Set<JobDefinitionEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
    }

    public async Task<JobDefinitionEntity> GetByNameAsync(string jobName, CancellationToken ct)
    {
        var normalized = jobName?.Trim();
        return await _context.Set<JobDefinitionEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == normalized && !x.IsDeleted, ct);
    }

    public async Task<ListPage<JobDefinitionEntity>> GetPagedAsync(PagedFilter filter, CancellationToken ct)
    {
        // Accept JobPagedFilter or fallback to PagedFilter
        var jobFilter = filter as JobPagedFilter ?? new JobPagedFilter(filter?.Search, filter.IsActive, filter?.PageNumber, filter?.PageSize, filter?.SortBy, filter?.SortDirection);

        var query = _context.Set<JobDefinitionEntity>()
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(jobFilter.Search))
        {
            var search = jobFilter.Search.Trim().ToLower();
            query = query.Where(x => EF.Functions.Like(x.Name.ToLower(), $"%{search}%"));
        }

        if (!string.IsNullOrWhiteSpace(jobFilter.Category))
        {
            query = query.Where(x => x.Category == jobFilter.Category);
        }

        if (jobFilter.IsActive.HasValue)
            query = query.Where(x => x.IsActive == jobFilter.IsActive.Value);

        if (jobFilter.IsSystemJob.HasValue)
            query = query.Where(x => x.IsSystemJob == jobFilter.IsSystemJob.Value);

        if (!string.IsNullOrWhiteSpace(jobFilter.Queue))
            query = query.Where(x => x.Queue == jobFilter.Queue);

        var count = await query.CountAsync(ct);

        var ordered = query.OrderBy(x => x.Category).ThenBy(x => x.Priority).ThenBy(x => x.Name);

        var pageNumber = jobFilter.PageNumber ?? 1;
        var pageSize = jobFilter.PageSize ?? Paging.MinPageSize();

        var data = await ordered.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return new ListPage<JobDefinitionEntity>
        {
            Items = data,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = count,
            TotalPages = (int)Math.Ceiling((double)count / pageSize)
        };
    }

    public async Task<bool> ExistsByNameAsync(string jobName, CancellationToken ct)
    {
        var normalized = jobName?.Trim();
        return await _context.Set<JobDefinitionEntity>()
            .AsNoTracking()
            .AnyAsync(x => x.Name == normalized && !x.IsDeleted, ct);
    }

    public async Task<bool> CreateAsync(JobDefinitionEntity entity, CancellationToken ct)
    {
        await _context.JobDefinitionEntities.AddAsync(entity, ct);
        return await _context.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> UpdateAsync(JobDefinitionEntity entity, CancellationToken ct)
    {
        _context.JobDefinitionEntities.Update(entity);
        return await _context.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> DeleteAsync(JobDefinitionEntity entity, CancellationToken ct)
    {
        _context.JobDefinitionEntities.Update(entity);
        return await _context.SaveChangesAsync(ct) > 0;
    }
}
