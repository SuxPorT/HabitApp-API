using Microsoft.AspNetCore.Mvc;
using HabitApp.Application.Dtos;
using HabitApp.Application.Interfaces;

namespace HabitApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HabitController(IHabitApplicationService applicationService) : ControllerBase
{
    private readonly IHabitApplicationService _applicationService = applicationService;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<HabitDto>>> Get()
    {
        var habits = await _applicationService.GetAllAsync();
        return Ok(habits);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HabitDto>> Get(int id)
    {
        var habit = await _applicationService.GetByIdAsync(id);
        if (habit == null) return NotFound();
        return Ok(habit);
    }

    [HttpPost]
    public async Task<ActionResult<HabitDto>> Post([FromBody] HabitDto habitDto)
    {
        if (habitDto == null) return BadRequest();
        var result = await _applicationService.AddAsync(habitDto);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<HabitDto>> Put(int id, [FromBody] HabitDto habitDto)
    {
        if (habitDto == null || id != habitDto.Id) return BadRequest();
        var result = await _applicationService.UpdateAsync(habitDto);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var deleted = await _applicationService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}