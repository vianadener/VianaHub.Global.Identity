namespace VianaHub.Global.Identity.Application.Dto.Request.Auth;

public class RegisterRequest
{
    public int TenantId { get; set; }
    public string Name { get; set; }
    public string Secret { get; set; }
    public string UrlImage { get; set; }
}
