namespace VianaHub.Global.Identity.Application.Dto.Response.App;

public record AppResponse(
    int Id,
    int TenantId,
    string Name,
    bool IsActive
);