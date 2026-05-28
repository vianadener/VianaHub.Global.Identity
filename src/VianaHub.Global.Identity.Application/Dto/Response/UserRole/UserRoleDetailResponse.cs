namespace VianaHub.Global.Identity.Application.Dto.Response.UserRole;

public record UserRoleDetailResponse(
     int Id,
     int TenantId,
     string Tenant,
     int UserId,
     string User,
     int RoleId,
     string Role,
     string UserName,
     string RoleName
);