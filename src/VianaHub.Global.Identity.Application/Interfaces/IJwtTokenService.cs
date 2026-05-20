using VianaHub.Global.Identity.Domain.Entities;

namespace VianaHub.Global.Identity.Application.Interfaces;

public interface IJwtTokenService
{
    Task<(string Token, DateTime ExpiresAt)> GenerateAccessTokenAsync(UserEntity user, CancellationToken ct);
}
