using ChugimCenter.Domain.Enums;

namespace ChugimCenter.Domain.Entities;

/// <summary>רישום של משתתפת לקבוצת חוג.</summary>
public class Enrollment
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int CourseGroupId { get; set; }
    public CourseGroup CourseGroup { get; set; } = null!;

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;

    public DateTime? CancelledAt { get; set; }
    public int? CancelledById { get; set; }

    // שדות התשלום יתווספו בשלב 4, יחד עם ה-Migration השנייה (AddPayment)
}
