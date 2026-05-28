namespace VianaHub.Global.Identity.Application.Dto.Request.User;

public record UpdateSecretRequest(
    string CurrentSecret,
    string NewSecret
);