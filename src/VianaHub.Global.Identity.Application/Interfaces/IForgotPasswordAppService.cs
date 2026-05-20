using VianaHub.Global.Identity.Application.Dto.Request.Auth;
using VianaHub.Global.Identity.Application.Dto.Response.Auth;

namespace VianaHub.Global.Identity.Application.Interfaces;

public interface IForgotPasswordAppService
{
    Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct);
    Task<ValidateResetTokenResponse> ValidateResetTokenAsync(ValidateResetTokenRequest request, CancellationToken ct);
    Task<ResetPasswordResponse> ResetPasswordAsync(ResetPasswordRequest request, string ipAddress, string userAgent, CancellationToken ct);
}
