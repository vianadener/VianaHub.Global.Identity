namespace VianaHub.Global.Identity.Application.Dto.Response.App;

public record AppDetailResponse(int Id, int TenantId, string Name, string Description, bool IsActive);
