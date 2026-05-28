using HabitApp.Domain.Entities;

namespace HabitApp.Application.Dtos;

public class HabitDto : BaseDto
{
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int Streak { get; set; }
    public bool[] CompletedDays { get; set; } = new bool[7];

    public int UserId { get; set; }
    public User? User { get; set; }
}