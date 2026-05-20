namespace VianaHub.Global.Identity.Application.Dto.Response.User;

public class UserDetailResponse
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Tenant { get; set; }
    public string Name { get; set; }
    public string UrlImage { get; set; }
    public DateTime? LastAccessAt { get; set; }
    public bool IsActive { get; set; }
}
