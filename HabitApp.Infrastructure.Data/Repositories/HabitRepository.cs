using Microsoft.EntityFrameworkCore;
using HabitApp.Domain.Entities;
using HabitApp.Infrastructure.Data.Context;
using HabitApp.Infrastructure.Data.Interfaces;

namespace HabitApp.Infrastructure.Data.Repositories;

public class HabitRepository(SqliteContext context) : IHabitRepository
{
    private readonly SqliteContext _context = context;

    public async Task<IEnumerable<Habit>> GetAllAsync()
        => await _context.Habits.ToListAsync();

    public async Task<Habit?> GetByIdAsync(int id)
        => await _context.Habits.FindAsync(id);

    public async Task<Habit> AddAsync(Habit habit)
    {
        await _context.Habits.AddAsync(habit);
        await _context.SaveChangesAsync();
        return habit;
    }

    public async Task<Habit> UpdateAsync(Habit habit)
    {
        _context.Entry(habit).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return habit;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var habit = await GetByIdAsync(id);
        if (habit == null) return false;

        habit.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }
}