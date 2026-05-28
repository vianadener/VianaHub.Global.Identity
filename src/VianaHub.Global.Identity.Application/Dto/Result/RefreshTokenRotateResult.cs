using VianaHub.Global.Identity.Domain.Entities;

namespace VianaHub.Global.Identity.Application.Dto.Result;

public record RefreshTokenRotateResult(
    RefreshTokenEntity OldEntity,
    string NewToken,
    RefreshTokenEntity NewEntity
);
