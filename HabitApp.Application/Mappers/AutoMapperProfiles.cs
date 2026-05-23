using AutoMapper;
using HabitApp.Application.Dtos;
using HabitApp.Domain.Entities;

namespace HabitApp.Application.Mappers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<Habit, HabitDto>()
            .ForMember(dest => dest.CompletedDays, opt => opt.MapFrom(src =>
                string.IsNullOrEmpty(src.CompletedDaysRaw)
                    ? new bool[7]
                    : src.CompletedDaysRaw.Split(',', StringSplitOptions.None).Select(bool.Parse).ToArray()
            ));

        CreateMap<HabitDto, Habit>()
            .ForMember(dest => dest.CompletedDaysRaw, opt => opt.MapFrom(src =>
                src.CompletedDays == null
                    ? "false,false,false,false,false,false,false"
                    : string.Join(",", src.CompletedDays.Select(b => b.ToString().ToLower()))
            ));
    }
}