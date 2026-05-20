namespace VianaHub.Global.Identity.Application.Dto.Response.Resource;

public class ResourceDetailResponse
{
    public int Id { get; set; }
    public int AppId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
}
