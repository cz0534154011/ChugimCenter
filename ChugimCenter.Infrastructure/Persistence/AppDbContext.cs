using ChugimCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChugimCenter.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<User> Users => Set<User>();
    public DbSet<CourseGroup> CourseGroups => Set<CourseGroup>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // טוען את כל קבצי ההגדרות (Configurations) שבפרויקט הזה
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyCourseGroupRules();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyCourseGroupRules();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// לפני כל שמירה של קבוצה: גרסה חדשה (ה-concurrency token), ויום בשבוע לפי תאריך ההתחלה.
    /// כתוב פעם אחת כאן, כדי שאף שירות לא יצטרך לזכור לעשות זאת.
    /// </summary>
    private void ApplyCourseGroupRules()
    {
        foreach (var entry in ChangeTracker.Entries<CourseGroup>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.Version = Guid.NewGuid();
                entry.Entity.DayOfWeek = entry.Entity.StartDate.DayOfWeek;
            }
        }
    }
}
