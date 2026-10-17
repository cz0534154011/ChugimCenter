namespace ChugimCenter.Application.DTOs.Users;

/// <summary>תלמידה ברשימת התלמידות של קבוצה, כפי שהמדריכה והמנהלת רואות.</summary>
public class StudentDto
{
    public int EnrollmentId { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateOnly? BirthDate { get; set; }
    public string? ParentName { get; set; }
    public string? ParentPhone { get; set; }
    public DateTime EnrolledAt { get; set; }
}
