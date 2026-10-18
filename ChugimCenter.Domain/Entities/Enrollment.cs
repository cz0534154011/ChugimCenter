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

    // שדות התשלום – נוספו ב-Migration השנייה (AddPayment)
    public bool IsPaid { get; set; }        // ברירת מחדל: לא שולם
    public DateTime? PaidAt { get; set; }   // מתי סומן התשלום, ב-UTC
    public int? PaidById { get; set; }      // המנהלת שסימנה
}
