using AutoMapper;
using HabitApp.Application.Dtos;
using HabitApp.Application.Interfaces;
using HabitApp.Domain.Entities;
using HabitApp.Domain.Services.Interfaces;

namespace HabitApp.Application;

public class HabitApplicationService(IHabitService habitService, IMapper mapper)
    : BaseApplicationService<Habit, HabitDto>(habitService, mapper), IHabitApplicationService
{
}