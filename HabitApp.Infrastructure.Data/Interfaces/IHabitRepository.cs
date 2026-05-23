using HabitApp.Domain.Entities;

namespace HabitApp.Infrastructure.Data.Interfaces;

public interface IHabitRepository
{
    Task<IEnumerable<Habit>> GetAllAsync();
    Task<Habit?> GetByIdAsync(int id);
    Task<Habit> AddAsync(Habit habit);
    Task<Habit> UpdateAsync(Habit habit);
    Task<bool> DeleteAsync(int id);
}