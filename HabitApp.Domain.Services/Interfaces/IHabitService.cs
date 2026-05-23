using HabitApp.Domain.Entities;

namespace HabitApp.Domain.Services.Interfaces;

public interface IHabitService
{
    Task<IEnumerable<Habit>> GetAllAsync();
    Task<Habit?> GetByIdAsync(int id);
    Task<Habit> AddAsync(Habit habit);
    Task<Habit> UpdateAsync(Habit habit);
    Task<bool> DeleteAsync(int id);
}