namespace VianaHub.Global.Identity.Application.Dto.Request.User;

public class CreateUserRequest
{
    public string Name { get; set; }
    public string Secret { get; set; }
    public string ConfirmSecret { get; set; }
    public string UrlImage { get; set; }
}
