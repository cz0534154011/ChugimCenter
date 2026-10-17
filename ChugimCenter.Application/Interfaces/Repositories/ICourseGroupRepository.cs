using ChugimCenter.Application.DTOs.Common;
using ChugimCenter.Application.DTOs.CourseGroups;
using ChugimCenter.Domain.Entities;

namespace ChugimCenter.Application.Interfaces.Repositories;

public interface ICourseGroupRepository : IRepository<CourseGroup>
{
    /// <summary>קבוצה עם התחום והמדריכה, לקריאה בלבד.</summary>
    Task<CourseGroup?> GetDetailsAsync(int id, CancellationToken ct);

    /// <summary>רשימה מסוננת וממוינת עם pagination, בתוך השאילתה.</summary>
    Task<PagedResult<CourseGroup>> GetPagedAsync(
        CourseGroupQueryDto query, bool includeInactive, DateOnly today, CancellationToken ct);

    Task<List<CourseGroup>> GetByInstructorAsync(int instructorId, CancellationToken ct);

    /// <summary>הקבוצות הפעילות של מדריכה ביום מסוים, לבדיקת חפיפה בזמן.</summary>
    Task<List<CourseGroup>> GetInstructorGroupsOnDayAsync(
        int instructorId, DayOfWeek day, int? excludeGroupId, CancellationToken ct);

    Task<bool> HasEnrollmentsAsync(int groupId, CancellationToken ct);
}
