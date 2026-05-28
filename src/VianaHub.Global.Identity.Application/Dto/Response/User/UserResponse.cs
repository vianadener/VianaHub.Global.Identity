namespace VianaHub.Global.Identity.Application.Dto.Response.User;

public record UserResponse(
     int Id,
     string Name,
     string PhoneNumber,
     DateTime? LastAccessAt,
     bool IsActive
);