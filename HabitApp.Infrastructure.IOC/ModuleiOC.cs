using HabitApp.Application;
using HabitApp.Application.Interfaces;
using HabitApp.Domain.Services;
using HabitApp.Domain.Services.Interfaces;
using HabitApp.Infrastructure.Data.Interfaces;
using HabitApp.Infrastructure.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace HabitApp.Infrastructure.IOC;

public static class ModuleIOC
{
    public static void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<IHabitApplicationService, HabitApplicationService>();
        services.AddScoped<IHabitService, HabitService>();
        services.AddScoped<IHabitRepository, HabitRepository>();
    }
}