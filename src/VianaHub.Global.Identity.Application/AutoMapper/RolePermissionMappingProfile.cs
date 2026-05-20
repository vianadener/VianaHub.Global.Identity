using AutoMapper;
using VianaHub.Global.Identity.Application.Dto.Base;
using VianaHub.Global.Identity.Application.Dto.Response.RolePermission;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Tools.Pagination;

namespace VianaHub.Global.Identity.Application.AutoMapper;

public class RolePermissionMappingProfile : Profile
{
    public RolePermissionMappingProfile()
    {
        CreateMap<RolePermissionEntity, RolePermissionResponse>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role != null ? src.Role.Name : string.Empty))
            .ForMember(dest => dest.Resource, opt => opt.MapFrom(src => src.Resource != null ? src.Resource.Name : string.Empty))
            .ForMember(dest => dest.Action, opt => opt.MapFrom(src => src.Action != null ? src.Action.Name : string.Empty));

        CreateMap<RolePermissionEntity, RolePermissionDetailResponse>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role != null ? src.Role.Name : string.Empty))
            .ForMember(dest => dest.Resource, opt => opt.MapFrom(src => src.Resource != null ? src.Resource.Name : string.Empty))
            .ForMember(dest => dest.Action, opt => opt.MapFrom(src => src.Action != null ? src.Action.Name : string.Empty));

        CreateMap<ListPage<RolePermissionEntity>, ListPageResponse<RolePermissionResponse>>();
    }
}
