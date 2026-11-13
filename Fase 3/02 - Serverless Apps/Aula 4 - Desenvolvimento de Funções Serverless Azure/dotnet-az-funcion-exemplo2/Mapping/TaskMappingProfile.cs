using AutoMapper;
using TaskManagement.Functions.Models;
using TaskManagement.Functions.Models.DTOs;
using TaskStatus = TaskManagement.Functions.Models.TaskStatus;

namespace TaskManagement.Functions.Mapping
{
    public class TaskMappingProfile : Profile
    {
        public TaskMappingProfile()
        {
            CreateMap<CreateTaskDto, TaskModel>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => TaskStatus.Pending))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CompletedAt, opt => opt.Ignore());

            CreateMap<TaskModel, TaskResponseDto>();

            CreateMap<UpdateTaskDto, TaskModel>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CompletedAt, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
