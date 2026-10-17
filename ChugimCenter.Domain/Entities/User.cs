using ChugimCenter.Domain.Enums;

namespace ChugimCenter.Domain.Entities;

/// <summary>
/// משתמשת במערכת: מנהלת, מדריכה או משתתפת.
/// אצל משתתפת, החשבון הוא של הילדה, וכולל גם את פרטי ההורה.
/// </summary>
public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }

    // מדריכה בלבד
    public string? Phone { get; set; }

    // משתתפת בלבד
    public DateOnly? BirthDate { get; set; }
    public string? ParentName { get; set; }
    public string? ParentPhone { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // קשר one-to-many: מדריכה מדריכה הרבה קבוצות
    public ICollection<CourseGroup> InstructedGroups { get; set; } = new List<CourseGroup>();

    // קשר one-to-many: למשתתפת יש הרבה רישומים (וכך many-to-many מול הקבוצות)
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
