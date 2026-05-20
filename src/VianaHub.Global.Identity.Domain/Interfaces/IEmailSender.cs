namespace VianaHub.Global.Identity.Domain.Interfaces;

public interface IEmailSender
{
    Task SendPasswordResetAsync(string toEmail, string toName, string resetLink, CancellationToken ct);
    Task SendPasswordResetConfirmationAsync(string toEmail, string toName, CancellationToken ct);
}
