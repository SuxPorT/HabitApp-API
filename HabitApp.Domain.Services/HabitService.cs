using HabitApp.Domain.Entities;
using HabitApp.Domain.Services.Interfaces;
using HabitApp.Infrastructure.Data.Interfaces;

namespace HabitApp.Domain.Services;

public class HabitService(IHabitRepository repository) : IHabitService
{
    private readonly IHabitRepository _repository = repository;

    public async Task<IEnumerable<Habit>> GetAllAsync()
        => await _repository.GetAllAsync();

    public async Task<Habit?> GetByIdAsync(int id)
        => await _repository.GetByIdAsync(id);

    public async Task<Habit> AddAsync(Habit habit)
    {
        habit.CreatedAt = DateTime.UtcNow;
        habit.IsDeleted = false;
        return await _repository.AddAsync(habit);
    }

    public async Task<Habit> UpdateAsync(Habit habit)
    {
        habit.ModifiedAt = DateTime.UtcNow;
        return await _repository.UpdateAsync(habit);
    }

    public async Task<bool> DeleteAsync(int id)
        => await _repository.DeleteAsync(id);
}