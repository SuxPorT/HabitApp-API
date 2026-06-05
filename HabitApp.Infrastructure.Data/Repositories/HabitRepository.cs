using HabitApp.Domain.Entities;
using HabitApp.Infrastructure.Data.Context;
using HabitApp.Infrastructure.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HabitApp.Infrastructure.Data.Repositories;

public class HabitRepository(SqliteContext context) 
    : BaseRepository<Habit>(context), IHabitRepository
{
    public async Task<IEnumerable<Habit>> GetByUserIdAsync(int userId)
        => await _context.Habits
            .Where(habit => habit.UserId == userId)
            .ToListAsync();
}
