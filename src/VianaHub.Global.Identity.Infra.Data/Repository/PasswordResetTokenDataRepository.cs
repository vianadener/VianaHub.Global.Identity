using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace VianaHub.Global.Identity.Infra.Data.Repository;

public class PasswordResetTokenDataRepository : IPasswordResetTokenDataRepository
{
    private readonly IdentityDbContext _context;

    public PasswordResetTokenDataRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<PasswordResetTokenEntity?> GetByTokenHashAsync(byte[] tokenHash, CancellationToken ct)
    {
        return await _context.Set<PasswordResetTokenEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, ct);
    }

    public async Task<int> CountRecentByUserAsync(int userId, int tenantId, DateTime since, CancellationToken ct)
    {
        return await _context.Set<PasswordResetTokenEntity>()
            .AsNoTracking()
            .CountAsync(x => x.UserId == userId && x.TenantId == tenantId && x.AddedOn >= since, ct);
    }

    public async Task<bool> CreateAsync(PasswordResetTokenEntity entity, CancellationToken ct)
    {
        await _context.Set<PasswordResetTokenEntity>().AddAsync(entity, ct);
        return await _context.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> UpdateAsync(PasswordResetTokenEntity entity, CancellationToken ct)
    {
        _context.Set<PasswordResetTokenEntity>().Update(entity);
        return await _context.SaveChangesAsync(ct) > 0;
    }
}
