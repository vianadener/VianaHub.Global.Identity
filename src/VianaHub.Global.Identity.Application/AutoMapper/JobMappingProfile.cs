using AutoMapper;
using VianaHub.Global.Identity.Application.Dto.Base;
using VianaHub.Global.Identity.Application.Dto.Response.Job;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Tools.Pagination;

namespace VianaHub.Global.Identity.Application.AutoMapper;

public class JobMappingProfile : Profile
{
    public JobMappingProfile()
    {
        CreateMap<JobDefinitionEntity, JobResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.JobCategory, opt => opt.MapFrom(src => src.Category))
            .ForMember(dest => dest.JobName, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.CronExpression, opt => opt.MapFrom(src => src.CronExpression))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

        CreateMap<JobDefinitionEntity, JobDetailResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.JobCategory, opt => opt.MapFrom(src => src.Category))
            .ForMember(dest => dest.JobName, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.JobPurpose, opt => opt.MapFrom(src => src.Purpose))
            .ForMember(dest => dest.JobType, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.JobMethod, opt => opt.MapFrom(src => src.Method))
            .ForMember(dest => dest.CronExpression, opt => opt.MapFrom(src => src.CronExpression))
            .ForMember(dest => dest.TimeZoneId, opt => opt.MapFrom(src => src.TimeZoneId))
            .ForMember(dest => dest.ExecuteOnlyOnce, opt => opt.MapFrom(src => src.ExecuteOnlyOnce))
            .ForMember(dest => dest.TimeoutMinutes, opt => opt.MapFrom(src => src.TimeoutMinutes))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority))
            .ForMember(dest => dest.Queue, opt => opt.MapFrom(src => src.Queue))
            .ForMember(dest => dest.MaxRetries, opt => opt.MapFrom(src => src.MaxRetries))
            .ForMember(dest => dest.JobConfiguration, opt => opt.MapFrom(src => src.Configuration))
            .ForMember(dest => dest.IsSystemJob, opt => opt.MapFrom(src => src.IsSystemJob))
            .ForMember(dest => dest.HangfireJobId, opt => opt.MapFrom(src => src.HangfireJobId))
            .ForMember(dest => dest.LastRegisteredAt, opt => opt.MapFrom(src => src.LastRegisteredAt))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.TenantId, opt => opt.Ignore())
            .ForMember(dest => dest.Tenant, opt => opt.Ignore())
            .ForMember(dest => dest.NextExecution, opt => opt.Ignore())
            .ForMember(dest => dest.LastExecution, opt => opt.Ignore())
            .ForMember(dest => dest.LastExecutionStatus, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore());

        CreateMap<ListPage<JobDefinitionEntity>, ListPageResponse<JobResponse>>();
    }
}
