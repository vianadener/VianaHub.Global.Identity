namespace VianaHub.Global.Identity.Application.Dto.Response.App;

public class AppDetailResponse
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
}
