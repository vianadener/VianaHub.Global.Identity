namespace VianaHub.Global.Identity.Application.Dto.Response.Tenant;

public record TenantDetailResponse(
    int Id,
    string Name,
    string Description,
    string Alias,
    string UrlImage,
    string Settings,
    string Remarks,
    bool IsActive
);