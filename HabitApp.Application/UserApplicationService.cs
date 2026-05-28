using AutoMapper;
using HabitApp.Application.Dtos;
using HabitApp.Application.Interfaces;
using HabitApp.Domain.Entities;
using HabitApp.Domain.Services.Interfaces;

namespace HabitApp.Application;

public class UserApplicationService(IUserService userService, IMapper _mapper) : IUserApplicationService
{
    private readonly IUserService _userService = userService;
    private readonly IMapper _mapper = _mapper;

    public async Task<UserResponseDto?> AuthenticateAsync(LoginRequestDto loginDto)
    {
        var user = await _userService.AuthenticateAsync(loginDto.Email, loginDto.Password);
        return _mapper.Map<UserResponseDto>(user);
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = await _userService.GetAllAsync();
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> AddAsync(UserDto userDto)
    {
        var user = _mapper.Map<User>(userDto);
        var result = await _userService.AddAsync(user);
        return _mapper.Map<UserDto>(result);
    }

    public async Task<UserDto> UpdateAsync(UserDto userDto)
    {
        var user = _mapper.Map<User>(userDto);
        var result = await _userService.UpdateAsync(user);
        return _mapper.Map<UserDto>(result);
    }

    public async Task<bool> DeleteAsync(int id)
        => await _userService.DeleteAsync(id);
}
