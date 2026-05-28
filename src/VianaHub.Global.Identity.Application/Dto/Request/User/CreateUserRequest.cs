namespace VianaHub.Global.Identity.Application.Dto.Request.User;

public record CreateUserRequest(
    string Name,
    string Secret,
    string ConfirmSecret,
    string UrlImage
);