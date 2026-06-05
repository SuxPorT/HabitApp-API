using HabitApp.Domain.Entities;

namespace HabitApp.Domain.Services.Interfaces;

public interface IHabitService : IBaseService<Habit>
{
    Task<IEnumerable<Habit>> GetByUserIdAsync(int userId);
}
