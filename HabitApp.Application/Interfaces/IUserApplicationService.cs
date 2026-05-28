using HabitApp.Application.Dtos;
using HabitApp.Domain.Entities;

namespace HabitApp.Application.Interfaces
{
    public interface IUserApplicationService
    {
        Task<UserResponseDto?> AuthenticateAsync(LoginRequestDto loginDto);
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<UserDto?> GetByIdAsync(int id);
        Task<UserDto> AddAsync(UserDto userDto);
        Task<UserDto> UpdateAsync(UserDto userDto);
        Task<bool> DeleteAsync(int id);
    }
}
