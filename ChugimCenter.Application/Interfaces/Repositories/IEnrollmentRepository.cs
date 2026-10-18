using ChugimCenter.Application.DTOs.Common;
using ChugimCenter.Application.DTOs.Enrollments;
using ChugimCenter.Domain.Entities;

namespace ChugimCenter.Application.Interfaces.Repositories;

public interface IEnrollmentRepository : IRepository<Enrollment>
{
    /// <summary>רישום יחד עם המשתתפת, הקבוצה והתחום, במצב מעקב (לביטול ולסימון תשלום).</summary>
    Task<Enrollment?> GetWithGroupAsync(int id, CancellationToken ct);

    Task<bool> HasActiveEnrollmentAsync(int userId, int groupId, CancellationToken ct);

    /// <summary>הקבוצות שבהן למשתתפת רישום פעיל ביום מסוים, לבדיקת חפיפה בזמן.</summary>
    Task<List<CourseGroup>> GetActiveGroupsOfUserOnDayAsync(int userId, DayOfWeek day, CancellationToken ct);

    /// <summary>רשימת רישומים מסוננת. userId מגביל לרישומים של משתתפת אחת.</summary>
    Task<PagedResult<Enrollment>> GetPagedAsync(EnrollmentQueryDto query, int? userId, CancellationToken ct);

    /// <summary>הרישומים הפעילים בקבוצה, עם פרטי המשתתפות.</summary>
    Task<List<Enrollment>> GetActiveByGroupAsync(int groupId, CancellationToken ct);
}
