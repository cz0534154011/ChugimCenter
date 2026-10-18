using ChugimCenter.Domain.Entities;
using ChugimCenter.Domain.Enums;

namespace ChugimCenter.Infrastructure.Persistence;

/// <summary>
/// נתוני דמו שנוצרים יחד עם בסיס הנתונים, כדי שאפשר יהיה להריץ את המערכת מיד.
/// הערכים קבועים (לא אקראיים), כדי שה-Migration לא ישתנה בכל בנייה.
///
/// פרטי כניסה לדמו (יופיעו גם ב-README):
///   מנהלת:    admin@example.com    / Admin1234!
///   מדריכות:  sara@example.com, miriam@example.com     / Teacher1234!
///   משתתפות:  chana@example.com, esther@example.com, leah@example.com / Member1234!
/// הסיסמאות שמורות כ-Hash של BCrypt בלבד.
/// </summary>
internal static class SeedData
{
    private const string AdminHash = "$2a$11$nNI43eulvTnlj41G1OvBwu029P..M.PiRacBxBeQlYRub2y73dFbu";
    private const string InstructorHash = "$2a$11$cSqSbGB5hx4kB36WlkVNM.sFeleoz4li5uM3xasATQMvEKzRL/QQq";
    private const string MemberHash = "$2a$11$8iUOwsheAj22pCaXjSc93eKJMzxcYLwWBGZ1YjGdvfGyJMXJgI.6u";

    private static readonly DateTime SeedTime = new(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);

    public static readonly Category[] Categories =
    [
        new() { Id = 1, Name = "ציור", IsActive = true },
        new() { Id = 2, Name = "דרמה", IsActive = true },
        new() { Id = 3, Name = "התעמלות", IsActive = true },
        new() { Id = 4, Name = "מוזיקה", IsActive = true }
    ];

    public static readonly User[] Users =
    [
        new() { Id = 1, FullName = "רבקה לוי", Email = "admin@example.com", PasswordHash = AdminHash,
                Role = UserRole.Admin, CreatedAt = SeedTime },

        new() { Id = 2, FullName = "שרה כהן", Email = "sara@example.com", PasswordHash = InstructorHash,
                Role = UserRole.Instructor, Phone = "050-1111111", CreatedAt = SeedTime },
        new() { Id = 3, FullName = "מרים פרידמן", Email = "miriam@example.com", PasswordHash = InstructorHash,
                Role = UserRole.Instructor, Phone = "052-2222222", CreatedAt = SeedTime },

        new() { Id = 4, FullName = "חנה גולד", Email = "chana@example.com", PasswordHash = MemberHash,
                Role = UserRole.Member, BirthDate = new DateOnly(2017, 3, 15),
                ParentName = "יעל גולד", ParentPhone = "054-3333333", CreatedAt = SeedTime },
        new() { Id = 5, FullName = "אסתר רוזן", Email = "esther@example.com", PasswordHash = MemberHash,
                Role = UserRole.Member, BirthDate = new DateOnly(2015, 6, 20),
                ParentName = "דבורה רוזן", ParentPhone = "053-4444444", CreatedAt = SeedTime },
        new() { Id = 6, FullName = "לאה שטרן", Email = "leah@example.com", PasswordHash = MemberHash,
                Role = UserRole.Member, BirthDate = new DateOnly(2016, 1, 10),
                ParentName = "רחל שטרן", ParentPhone = "058-5555555", CreatedAt = SeedTime }
    ];

    public static readonly CourseGroup[] CourseGroups =
    [
        Group(1, "ציור מתחילות", categoryId: 1, instructorId: 2, new DateOnly(2026, 11, 1), new TimeOnly(16, 0), 60,
              minAge: 6, maxAge: 9, price: 180, capacity: 12, enrolled: 0, "0a1b2c3d-0001-4000-8000-000000000001"),
        Group(2, "ציור מתקדמות", categoryId: 1, instructorId: 2, new DateOnly(2026, 11, 3), new TimeOnly(17, 0), 75,
              minAge: 10, maxAge: 13, price: 200, capacity: 10, enrolled: 0, "0a1b2c3d-0002-4000-8000-000000000002"),

        // הקבוצה להדגמת התחרות: נשאר בה מקום אחד, ושתי משתתפות (אסתר ולאה) מתאימות לגיל
        Group(3, "דרמה גילאי 8–11", categoryId: 2, instructorId: 3, new DateOnly(2026, 11, 2), new TimeOnly(16, 30), 60,
              minAge: 8, maxAge: 11, price: 190, capacity: 2, enrolled: 1, "0a1b2c3d-0003-4000-8000-000000000003"),

        Group(4, "התעמלות קרקע", categoryId: 3, instructorId: 3, new DateOnly(2026, 11, 4), new TimeOnly(16, 0), 45,
              minAge: 6, maxAge: 10, price: 170, capacity: 15, enrolled: 0, "0a1b2c3d-0004-4000-8000-000000000004"),
        Group(5, "מקהלת בנות", categoryId: 4, instructorId: 2, new DateOnly(2026, 11, 5), new TimeOnly(17, 30), 60,
              minAge: 8, maxAge: 14, price: 160, capacity: 20, enrolled: 0, "0a1b2c3d-0005-4000-8000-000000000005")
    ];

    // מספר הנרשמות בכל קבוצה (EnrolledCount) תואם בדיוק לרישומים הפעילים כאן
    public static readonly Enrollment[] Enrollments =
    [
        new() { Id = 1, UserId = 4, CourseGroupId = 3, EnrolledAt = SeedTime, Status = EnrollmentStatus.Active }
    ];

    private static CourseGroup Group(int id, string name, int categoryId, int instructorId,
        DateOnly startDate, TimeOnly startTime, int durationMinutes,
        int minAge, int maxAge, decimal price, int capacity, int enrolled, string version) => new()
    {
        Id = id,
        Name = name,
        CategoryId = categoryId,
        InstructorId = instructorId,
        StartDate = startDate,
        SessionsCount = 30,
        DayOfWeek = startDate.DayOfWeek,
        StartTime = startTime,
        DurationMinutes = durationMinutes,
        MinAge = minAge,
        MaxAge = maxAge,
        Price = price,
        Capacity = capacity,
        EnrolledCount = enrolled,
        IsActive = true,
        Version = Guid.Parse(version)
    };
}
