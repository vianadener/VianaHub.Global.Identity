namespace VianaHub.Global.Identity.Application.Dto.Response.Tenant;

public record TenantResponse(
     int Id,
     string Name,
     string Alias,
     bool IsActive
);

