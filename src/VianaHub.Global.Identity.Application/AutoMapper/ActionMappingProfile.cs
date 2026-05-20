using AutoMapper;
using VianaHub.Global.Identity.Application.Dto.Base;
using VianaHub.Global.Identity.Application.Dto.Response.Action;
using VianaHub.Global.Identity.Application.Dto.Response.Jwt;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Tools.Pagination;

namespace VianaHub.Global.Identity.Application.AutoMapper;

/// <summary>
/// Perfil de mapeamento do AutoMapper para ActionEntity
/// </summary>
public class ActionMappingProfile : Profile
{
    public ActionMappingProfile()
    {
        // Mapeia ActionEntity -> ActionResponse
        CreateMap<ActionEntity, ActionResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

        CreateMap<ActionEntity, ActionDetailResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

        CreateMap<ListPage<ActionEntity>, ListPageResponse<ActionResponse>>();

        CreateMap<JwtKeyEntity, JwtKeyResponse>();
        CreateMap<JwtKeyEntity, JwtKeyDetailResponse>()
            .ForMember(dest => dest.Tenant, opt => opt.Ignore());
    }
}
