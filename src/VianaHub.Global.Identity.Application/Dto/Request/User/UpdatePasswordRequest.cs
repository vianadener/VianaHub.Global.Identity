namespace VianaHub.Global.Identity.Application.Dto.Request.User;

public record UpdatePasswordRequest(
    string CurrentPassword,
    string NewPassword
);