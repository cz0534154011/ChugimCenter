namespace ChugimCenter.Application.DTOs.CourseGroups;

/// <summary>פרטי קבוצה שחוזרים מה-API, כולל שדות מחושבים.</summary>
public class CourseGroupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    public int InstructorId { get; set; }
    public string InstructorName { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }          // מחושב
    public int SessionsCount { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public int DurationMinutes { get; set; }

    public int MinAge { get; set; }
    public int MaxAge { get; set; }
    public decimal Price { get; set; }

    public int Capacity { get; set; }
    public int EnrolledCount { get; set; }
    public int AvailableSpots { get; set; }        // מחושב
    public bool IsActive { get; set; }
}
