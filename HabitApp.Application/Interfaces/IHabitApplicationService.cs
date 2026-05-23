using HabitApp.Application.Dtos;

namespace HabitApp.Application.Interfaces;

public interface IHabitApplicationService
{
    Task<IEnumerable<HabitDto>> GetAllAsync();
    Task<HabitDto?> GetByIdAsync(int id);
    Task<HabitDto> AddAsync(HabitDto habitDto);
    Task<HabitDto> UpdateAsync(HabitDto habitDto);
    Task<bool> DeleteAsync(int id);
}