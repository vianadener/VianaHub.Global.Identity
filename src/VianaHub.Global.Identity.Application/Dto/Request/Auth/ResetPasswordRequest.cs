namespace VianaHub.Global.Identity.Application.Dto.Request.Auth;

public record ResetPasswordRequest(string Token, string NewPassword, string ConfirmPassword);