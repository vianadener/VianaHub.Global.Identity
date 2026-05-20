using AutoMapper;
using VianaHub.Global.Identity.Application.Dto.Base;
using VianaHub.Global.Identity.Application.Dto.Response.App;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Tools.Pagination;

namespace VianaHub.Global.Identity.Application.AutoMapper;

public class AppMappingProfile : Profile
{
    public AppMappingProfile()
    {
        CreateMap<AppEntity, AppResponse>();
        CreateMap<AppEntity, AppDetailResponse>();
        CreateMap<ListPage<AppEntity>, ListPageResponse<AppResponse>>();
    }
}
