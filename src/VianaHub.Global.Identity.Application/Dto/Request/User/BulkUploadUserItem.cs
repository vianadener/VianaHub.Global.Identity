namespace VianaHub.Global.Identity.Application.Dto.Request.User;

public record BulkUploadUserItem(
    string Name,
    string Secret,
    string UrlImage
);