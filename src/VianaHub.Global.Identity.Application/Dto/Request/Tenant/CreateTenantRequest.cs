namespace VianaHub.Global.Identity.Application.Dto.Request.Tenant;

public record CreateTenantRequest(
    string Name,
    string Description,
    string Alias,
    string UrlImage,
    string Settings,
    string Remarks
);