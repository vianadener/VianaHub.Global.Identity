namespace VianaHub.Global.Identity.Application.Dto.Request.Tenant;

public record UpdateTenantRequest(
    string Name,
    string Description,
    string Alias,
    string UrlImage,
    string Settings,
    string Remarks
);