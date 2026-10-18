using ChugimCenter.Domain.Entities;
using ChugimCenter.Domain.Enums;
using Moq;

namespace ChugimCenter.Tests.Helpers;

/// <summary>
/// נתוני הבדיקות במקום אחד: "היום" קבוע, ותחום, קבוצה, משתתפת ורישום עם ערכים תקינים כברירת מחדל.
/// כל בדיקה משנה רק את מה שמיוחד לה, וכך אין קוד כפול בין הבדיקות.
/// </summary>
public static class TestData
{
    public const int AdminId = 1;
    public const int InstructorId = 2;
    public const int OtherInstructorId = 3;
    public const int MemberId = 4;

    // "היום" של הבדיקות: יום ראשון, 1/11/2026. קבוע, כדי שהבדיקות לא ייכשלו כשהזמן עובר
    public static readonly DateOnly Today = new(2026, 11, 1);

    // החוג מתחיל שבוע אחרי "היום", גם הוא ביום ראשון
    public static readonly DateOnly GroupStart = Today.AddDays(7);

    /// <summary>שעון מדומה שמחזיר תמיד את "היום" של הבדיקות.</summary>
    public static TimeProvider Clock()
    {
        var clock = new Mock<TimeProvider>();
        clock.Setup(c => c.GetUtcNow())
             .Returns(new DateTimeOffset(Today.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero));
        return clock.Object;
    }

    public static Category DramaCategory() => new() { Id = 2, Name = "דרמה", IsActive = true };

    /// <summary>קבוצת דרמה פעילה לגילאי 8–10, בימי ראשון 16:00–17:00.</summary>
    public static CourseGroup Group(int capacity = 2, int enrolledCount = 0)
    {
        var category = DramaCategory();
        return new CourseGroup
        {
            Id = 3,
            Name = "דרמה לגילאי 8–10",
            CategoryId = category.Id,
            Category = category,
            InstructorId = InstructorId,
            StartDate = GroupStart,
            DayOfWeek = GroupStart.DayOfWeek,
            SessionsCount = 10,
            StartTime = new TimeOnly(16, 0),
            DurationMinutes = 60,
            MinAge = 8,
            MaxAge = 10,
            Price = 120,
            Capacity = capacity,
            EnrolledCount = enrolledCount,
            IsActive = true
        };
    }

    /// <summary>משתתפת. ברירת המחדל: בת 9 בתאריך תחילת החוג, כלומר מתאימה לקבוצה.</summary>
    public static User Member(DateOnly? birthDate = null) => new()
    {
        Id = MemberId,
        FullName = "חנה גולד",
        Email = "chana@example.com",
        Role = UserRole.Member,
        BirthDate = birthDate ?? new DateOnly(2017, 5, 1),
        ParentName = "משפחת גולד",
        ParentPhone = "050-0000000"
    };

    /// <summary>רישום פעיל של משתתפת לקבוצה, עם הקבוצה והמשתתפת עצמן (כמו אחרי Include).</summary>
    public static Enrollment ActiveEnrollment(User member, CourseGroup group) => new()
    {
        Id = 10,
        UserId = member.Id,
        User = member,
        CourseGroupId = group.Id,
        CourseGroup = group,
        EnrolledAt = Today.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
        Status = EnrollmentStatus.Active
    };
}
