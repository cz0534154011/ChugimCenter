using ChugimCenter.Application.DTOs.Common;
using ChugimCenter.Application.DTOs.CourseGroups;
using ChugimCenter.Application.Enums;
using ChugimCenter.Application.Interfaces.Repositories;
using ChugimCenter.Domain.Entities;
using ChugimCenter.Infrastructure.Extensions;
using ChugimCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChugimCenter.Infrastructure.Repositories;

public class CourseGroupRepository(AppDbContext context) : Repository<CourseGroup>(context), ICourseGroupRepository
{
    public Task<CourseGroup?> GetDetailsAsync(int id, CancellationToken ct)
        => Set.AsNoTracking()
              .Include(g => g.Category)
              .Include(g => g.Instructor)
              .FirstOrDefaultAsync(g => g.Id == id, ct);

    public Task<PagedResult<CourseGroup>> GetPagedAsync(
        CourseGroupQueryDto query, bool includeInactive, DateOnly today, CancellationToken ct)
    {
        var groups = Set.AsNoTracking()
                        .Include(g => g.Category)
                        .Include(g => g.Instructor)
                        .AsQueryable();

        if (!includeInactive)
            groups = groups.Where(g => g.IsActive && g.Category.IsActive);

        if (query.CategoryId is not null)
            groups = groups.Where(g => g.CategoryId == query.CategoryId);

        if (query.InstructorId is not null)
            groups = groups.Where(g => g.InstructorId == query.InstructorId);

        if (query.DayOfWeek is not null)
            groups = groups.Where(g => g.DayOfWeek == query.DayOfWeek);

        if (query.Age is not null)
            groups = groups.Where(g => g.MinAge <= query.Age && g.MaxAge >= query.Age);

        if (query.OnlyAvailable)
            groups = groups.Where(g => g.EnrolledCount < g.Capacity);

        // חוג שהסתיים: תאריך המפגש האחרון (התחלה + שבוע לכל מפגש נוסף) כבר עבר
        if (!query.IncludeEnded)
            groups = groups.Where(g => g.StartDate.AddDays((g.SessionsCount - 1) * 7) >= today);

        groups = query.SortBy switch
        {
            CourseGroupSortBy.Name => query.Descending ? groups.OrderByDescending(g => g.Name) : groups.OrderBy(g => g.Name),
            CourseGroupSortBy.Price => query.Descending ? groups.OrderByDescending(g => g.Price) : groups.OrderBy(g => g.Price),
            _ => query.Descending ? groups.OrderByDescending(g => g.StartDate) : groups.OrderBy(g => g.StartDate)
        };

        return groups.ToPagedResultAsync(query.Page, query.PageSize, ct);
    }

    public Task<List<CourseGroup>> GetByInstructorAsync(int instructorId, CancellationToken ct)
        => Set.AsNoTracking()
              .Include(g => g.Category)
              .Include(g => g.Instructor)
              .Where(g => g.InstructorId == instructorId)
              .OrderBy(g => g.DayOfWeek)
              .ThenBy(g => g.StartTime)
              .ToListAsync(ct);

    public Task<List<CourseGroup>> GetInstructorGroupsOnDayAsync(
        int instructorId, DayOfWeek day, int? excludeGroupId, CancellationToken ct)
        => Set.AsNoTracking()
              .Where(g => g.InstructorId == instructorId
                          && g.IsActive
                          && g.DayOfWeek == day
                          && (excludeGroupId == null || g.Id != excludeGroupId))
              .ToListAsync(ct);

    public Task<bool> HasEnrollmentsAsync(int groupId, CancellationToken ct)
        => Context.Enrollments.AnyAsync(e => e.CourseGroupId == groupId, ct);
}
