using HabitApp.Domain.Entities;
using HabitApp.Domain.Services.Interfaces;
using HabitApp.Infrastructure.Data.Interfaces;

namespace HabitApp.Domain.Services;

public class HabitService(IHabitRepository repository) 
    : BaseService<Habit>(repository), IHabitService
{
    private readonly IHabitRepository _habitRepository = repository;

    public async Task<IEnumerable<Habit>> GetByUserIdAsync(int userId)
        => await _habitRepository.GetByUserIdAsync(userId);
}
