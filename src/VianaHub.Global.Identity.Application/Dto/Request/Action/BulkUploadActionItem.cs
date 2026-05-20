namespace VianaHub.Global.Identity.Application.Dto.Request.Action;

public class BulkUploadActionItem
{
    public int AppId { get; set; }
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}
