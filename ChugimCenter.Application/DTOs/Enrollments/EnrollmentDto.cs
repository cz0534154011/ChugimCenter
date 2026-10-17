using ChugimCenter.Domain.Enums;

namespace ChugimCenter.Application.DTOs.Enrollments;

/// <summary>פרטי רישום שחוזרים מה-API.</summary>
public class EnrollmentDto
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public string StudentName { get; set; } = string.Empty;

    public int CourseGroupId { get; set; }
    public string CourseGroupName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }

    public DateTime EnrolledAt { get; set; }
    public EnrollmentStatus Status { get; set; }
    public DateTime? CancelledAt { get; set; }

    public bool IsPaid { get; set; }
    public DateTime? PaidAt { get; set; }
}
