namespace VianaHub.Global.Identity.Application.Dto.Request.Tenant;

public class CreateTenantRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Alias { get; set; }
    public string UrlImage { get; set; }
    public string Settings { get; set; }
    public string Remarks { get; set; }
}
