using HabitApp.Application.Dtos;
using HabitApp.Application.Interfaces;

namespace HabitApp.API.Controllers;

public class HabitController(IHabitApplicationService applicationService)
    : BaseController<HabitDto>(applicationService)
{
}