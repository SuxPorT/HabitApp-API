using HabitApp.Domain.Entities;
using HabitApp.Infrastructure.Data.Context;
using HabitApp.Infrastructure.Data.Interfaces;

namespace HabitApp.Infrastructure.Data.Repositories;

public class HabitRepository(SqliteContext context) 
    : BaseRepository<Habit>(context), IHabitRepository
{
}
