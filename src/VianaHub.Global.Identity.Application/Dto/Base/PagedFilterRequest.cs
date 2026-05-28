using VianaHub.Global.Identity.Domain.Tools.Pagination;

namespace VianaHub.Global.Identity.Application.Dto.Base;

public record PagedFilterRequest : Paging
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; } = true;
}
