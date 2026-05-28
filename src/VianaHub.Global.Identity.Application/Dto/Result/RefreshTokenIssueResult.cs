using VianaHub.Global.Identity.Domain.Entities;

namespace VianaHub.Global.Identity.Application.Dto.Result;

public record RefreshTokenIssueResult(
    string Token,
    DateTime ExpiresAt,
    RefreshTokenEntity Entity
);
