using VianaHub.Global.Identity.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace VianaHub.Global.Identity.Infra.Integration.Email;

/// <summary>
/// Implementação no-op do IEmailSender. Substitua por um provedor real (SendGrid, SMTP, etc.).
/// </summary>
public class NoOpEmailSender : IEmailSender
{
    private readonly ILogger<NoOpEmailSender> _logger;

    public NoOpEmailSender(ILogger<NoOpEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendPasswordResetAsync(string toEmail, string toName, string resetLink, CancellationToken ct)
    {
        _logger.LogInformation(
            "NoOpEmailSender: email de reset de senha para {ToEmail} com link {ResetLink}",
            toEmail, resetLink);

        return Task.CompletedTask;
    }

    public Task SendPasswordResetConfirmationAsync(string toEmail, string toName, CancellationToken ct)
    {
        _logger.LogInformation(
            "NoOpEmailSender: email de confirmação de senha alterada para {ToEmail}",
            toEmail);

        return Task.CompletedTask;
    }
}
