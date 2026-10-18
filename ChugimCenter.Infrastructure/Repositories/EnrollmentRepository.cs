using ChugimCenter.Application.DTOs.Common;
using ChugimCenter.Application.DTOs.Enrollments;
using ChugimCenter.Application.Interfaces.Repositories;
using ChugimCenter.Domain.Entities;
using ChugimCenter.Domain.Enums;
using ChugimCenter.Infrastructure.Extensions;
using ChugimCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChugimCenter.Infrastructure.Repositories;

public class EnrollmentRepository(AppDbContext context) : Repository<Enrollment>(context), IEnrollmentRepository
{
    public Task<Enrollment?> GetWithGroupAsync(int id, CancellationToken ct)
        => Set.Include(e => e.CourseGroup)
              .FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task<bool> HasActiveEnrollmentAsync(int userId, int groupId, CancellationToken ct)
        => Set.AnyAsync(e => e.UserId == userId
                             && e.CourseGroupId == groupId
                             && e.Status == EnrollmentStatus.Active, ct);

    public Task<List<CourseGroup>> GetActiveGroupsOfUserOnDayAsync(int userId, DayOfWeek day, CancellationToken ct)
        => Set.AsNoTracking()
              .Where(e => e.UserId == userId
                          && e.Status == EnrollmentStatus.Active
                          && e.CourseGroup.DayOfWeek == day)
              .Select(e => e.CourseGroup)
              .ToListAsync(ct);

    public Task<PagedResult<Enrollment>> GetPagedAsync(EnrollmentQueryDto query, int? userId, CancellationToken ct)
    {
        var enrollments = Set.AsNoTracking()
                             .Include(e => e.User)
                             .Include(e => e.CourseGroup)
                                 .ThenInclude(g => g.Category)
                             .AsQueryable();

        if (userId is not null)
            enrollments = enrollments.Where(e => e.UserId == userId);

        if (query.CourseGroupId is not null)
            enrollments = enrollments.Where(e => e.CourseGroupId == query.CourseGroupId);

        if (query.Status is not null)
            enrollments = enrollments.Where(e => e.Status == query.Status);

        return enrollments
            .OrderByDescending(e => e.EnrolledAt)
            .ToPagedResultAsync(query.Page, query.PageSize, ct);
    }

    public Task<List<Enrollment>> GetActiveByGroupAsync(int groupId, CancellationToken ct)
        => Set.AsNoTracking()
              .Include(e => e.User)
              .Where(e => e.CourseGroupId == groupId && e.Status == EnrollmentStatus.Active)
              .OrderBy(e => e.User.FullName)
              .ToListAsync(ct);
}
