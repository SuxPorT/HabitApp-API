using HabitApp.Domain.Entities;

namespace HabitApp.Infrastructure.Data.Interfaces;

public interface IHabitRepository : IBaseRepository<Habit>
{
    Task<IEnumerable<Habit>> GetByUserIdAsync(int userId);
}
