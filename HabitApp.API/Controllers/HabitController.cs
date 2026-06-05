using HabitApp.Application.Dtos;
using HabitApp.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HabitApp.API.Controllers;

public class HabitController(IHabitApplicationService applicationService)
    : BaseController<HabitDto>(applicationService)
{
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<HabitDto>>> GetByUserId(int userId)
    {
        var habits = await applicationService.GetByUserIdAsync(userId);
        return Ok(habits);
    }
}
