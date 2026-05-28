namespace HabitApp.Domain.Entities;

public class Habit
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int Streak { get; set; }
    public string CompletedDaysRaw { get; set; } = "false,false,false,false,false,false,false";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }
    public bool IsDeleted { get; set; } = false;

    public int UserId { get; set; }
    public User? User { get; set; }
}