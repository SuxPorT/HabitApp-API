namespace HabitApp.Domain.Entities;

public class Habit : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int Streak { get; set; }
    public string CompletedDaysRaw { get; set; } = "false,false,false,false,false,false,false";

    public int UserId { get; set; }
    public User? User { get; set; }
}