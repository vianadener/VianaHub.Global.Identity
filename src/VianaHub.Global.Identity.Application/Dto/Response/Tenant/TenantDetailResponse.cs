namespace VianaHub.Global.Identity.Application.Dto.Response.Tenant;

public class TenantDetailResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Alias { get; set; }
    public string UrlImage { get; set; }
    public string Settings { get; set; }
    public string Remarks { get; set; }
    public bool IsActive { get; set; }
}
