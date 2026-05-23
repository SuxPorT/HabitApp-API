using HabitApp.Domain.Entities;
using HabitApp.Infrastructure.Data.Utils;
using Microsoft.EntityFrameworkCore;

namespace HabitApp.Infrastructure.Data.Context;

public class SqliteContext(DbContextOptions<SqliteContext> options) : DbContext(options)
{
    public DbSet<Habit> Habits { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Habit>().HasQueryFilter(h => !h.IsDeleted);
        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        ConfigureDates();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ConfigureDates();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ConfigureDates()
    {
        var entries = ChangeTracker.Entries<Habit>();
        var brazilDatetime = DateTimeUtils.GetHorarioBrasilia();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = brazilDatetime;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(x => x.CreatedAt).IsModified = false;
                entry.Entity.ModifiedAt = brazilDatetime;
            }
        }
    }
}