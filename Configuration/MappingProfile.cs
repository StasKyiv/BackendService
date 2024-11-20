using AutoMapper;
using BackendService.DTOs;
using BackendService.Entities;
using BackendService.Enums;

namespace BackendService.Configuration;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<WorkTaskDto, WorkTask>()
            .ForMember(dest => dest.Status,
                opt =>
                    opt.MapFrom(src => Enum.Parse<Status>(src.Status)));
        
        CreateMap<WorkTask, WorkTaskDto>()
            .ForMember(dest => dest.Status, 
                opt => opt.MapFrom<StatusEnumToNameResolver>());
    }

    private class StatusEnumToNameResolver : IValueResolver<WorkTask, WorkTaskDto, string>
    {
        public string Resolve(WorkTask source, WorkTaskDto destination, string destMember, ResolutionContext context)
        {
            // Use Enum.GetName to get the name of the enum value as a string
            return Enum.GetName(typeof(Status), source.Status); // Converts Status enum value to its name, e.g., "InProgress"
        }
    }
}