namespace VianaHub.Global.Identity.Application.Dto.Request.User;

public class UpdateSecretRequest
{
    public string CurrentSecret { get; set; }
    public string NewSecret { get; set; }
}
