namespace VianaHub.Global.Identity.Application.Dto.Response.Job;

public class JobResponse
{
    public int Id { get; set; }
    public string JobCategory { get; set; }
    public string JobName { get; set; }
    public string CronExpression { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; }
}
