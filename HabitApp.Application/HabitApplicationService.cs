using AutoMapper;
using HabitApp.Application.Dtos;
using HabitApp.Application.Interfaces;
using HabitApp.Domain.Entities;
using HabitApp.Domain.Services.Interfaces;

namespace HabitApp.Application;

public class HabitApplicationService(IHabitService habitService, IMapper mapper)
    : BaseApplicationService<Habit, HabitDto>(habitService, mapper), IHabitApplicationService
{
    private readonly IHabitService _habitService = habitService;

    public async Task<IEnumerable<HabitDto>> GetByUserIdAsync(int userId)
    {
        var habits = await _habitService.GetByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<HabitDto>>(habits);
    }
}
