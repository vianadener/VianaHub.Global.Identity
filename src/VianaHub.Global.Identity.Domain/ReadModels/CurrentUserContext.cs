namespace VianaHub.Global.Identity.Domain.ReadModels;

public class CurrentUserContext
{
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
}
