namespace VianaHub.Global.Identity.Application.Dto.Response.Jwt;

public class JwtKeyResponse
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Guid KeyId { get; set; }
    public string PublicKey { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
