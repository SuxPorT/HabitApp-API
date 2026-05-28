using Microsoft.AspNetCore.Mvc;
using HabitApp.Application.Dtos;
using HabitApp.Application.Interfaces;

namespace HabitApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IUserApplicationService userApplicationService) : ControllerBase
{
    private readonly IUserApplicationService _userApplicationService = userApplicationService;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> Get()
    {
        var users = await _userApplicationService.GetAllAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> Get(int id)
    {
        var user = await _userApplicationService.GetByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Post([FromBody] UserDto userDto)
    {
        if (userDto == null) return BadRequest();
        var result = await _userApplicationService.AddAsync(userDto);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserResponseDto>> Login([FromBody] LoginRequestDto loginDto)
    {
        if (loginDto == null) return BadRequest();
        var result = await _userApplicationService.AuthenticateAsync(loginDto);
        if (result == null) return Unauthorized("Credenciais inválidas.");
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> Put(int id, [FromBody] UserDto userDto)
    {
        if (userDto == null || id != userDto.Id) return BadRequest();
        var result = await _userApplicationService.UpdateAsync(userDto);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var deleted = await _userApplicationService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}