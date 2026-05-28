namespace VianaHub.Global.Identity.Application.Dto.Response.User;

public record UserDetailResponse(
     int Id,
     int TenantId,
     string Tenant,
     string Name,
     string UrlImage,
     DateTime? LastAccessAt,
     bool IsActive
);
