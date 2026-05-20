using VianaHub.Global.Identity.Domain.Entities;

namespace VianaHub.Global.Identity.Application.Dto.Result;

public class RefreshTokenRotateResult
{
    public RefreshTokenEntity OldEntity { get; init; }
    public string NewToken { get; init; }
    public RefreshTokenEntity NewEntity { get; init; }
}
