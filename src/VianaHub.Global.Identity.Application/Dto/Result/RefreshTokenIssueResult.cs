using VianaHub.Global.Identity.Domain.Entities;

namespace VianaHub.Global.Identity.Application.Dto.Result;

public class RefreshTokenIssueResult
{
    public string Token { get; init; }
    public DateTime ExpiresAt { get; init; }
    public RefreshTokenEntity Entity { get; init; }
}
