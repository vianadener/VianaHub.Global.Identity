namespace VianaHub.Global.Identity.Domain.Interfaces;

public interface IRefreshTokenHasher
{
    byte[] Hash(string refreshToken);
}
