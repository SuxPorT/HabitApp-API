using HabitApp.Domain.Entities;
using HabitApp.Infrastructure.Data.Utils;
using Microsoft.EntityFrameworkCore;

namespace HabitApp.Infrastructure.Data.Context;

public class SqliteContext(DbContextOptions<SqliteContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Habit> Habits { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<Habit>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(h => h.User)
                  .WithMany(u => u.Habits)
                  .HasForeignKey(h => h.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>().HasQueryFilter(h => !h.IsDeleted);
        modelBuilder.Entity<Habit>().HasQueryFilter(h => !h.IsDeleted);
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