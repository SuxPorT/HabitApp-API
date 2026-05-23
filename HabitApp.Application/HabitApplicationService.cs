using AutoMapper;
using HabitApp.Application.Dtos;
using HabitApp.Application.Interfaces;
using HabitApp.Domain.Entities;
using HabitApp.Domain.Services.Interfaces;

namespace HabitApp.Application;

public class HabitApplicationService(IHabitService habitService, IMapper _mapper) : IHabitApplicationService
{
    private readonly IHabitService _habitService = habitService;
    private readonly IMapper _mapper = _mapper;

    public async Task<IEnumerable<HabitDto>> GetAllAsync()
    {
        var habits = await _habitService.GetAllAsync();
        return _mapper.Map<IEnumerable<HabitDto>>(habits);
    }

    public async Task<HabitDto?> GetByIdAsync(int id)
    {
        var habit = await _habitService.GetByIdAsync(id);
        return _mapper.Map<HabitDto>(habit);
    }

    public async Task<HabitDto> AddAsync(HabitDto habitDto)
    {
        var habit = _mapper.Map<Habit>(habitDto);
        var result = await _habitService.AddAsync(habit);
        return _mapper.Map<HabitDto>(result);
    }

    public async Task<HabitDto> UpdateAsync(HabitDto habitDto)
    {
        var habit = _mapper.Map<Habit>(habitDto);
        var result = await _habitService.UpdateAsync(habit);
        return _mapper.Map<HabitDto>(result);
    }

    public async Task<bool> DeleteAsync(int id)
        => await _habitService.DeleteAsync(id);
}