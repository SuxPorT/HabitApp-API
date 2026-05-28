using HabitApp.Domain.Entities;
using HabitApp.Domain.Services.Interfaces;
using HabitApp.Infrastructure.Data.Interfaces;

namespace HabitApp.Domain.Services;

public class HabitService(IHabitRepository repository) 
    : BaseService<Habit>(repository), IHabitService
{
}
