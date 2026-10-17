using System.ComponentModel.DataAnnotations;

namespace ChugimCenter.Application.DTOs.CourseGroups;

/// <summary>
/// יצירה או עדכון של קבוצת חוג.
/// אין כאן מזהה, יום בשבוע, מספר נרשמות, סטטוס או גרסה: אותם השרת קובע.
/// </summary>
public class CourseGroupRequestDto
{
    [Required(ErrorMessage = "שם הקבוצה הוא שדה חובה")]
    [MaxLength(100, ErrorMessage = "שם הקבוצה יכול להכיל עד 100 תווים")]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "יש לבחור תחום")]
    public int CategoryId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "יש לבחור מדריכה")]
    public int InstructorId { get; set; }

    [Required(ErrorMessage = "תאריך ההתחלה הוא שדה חובה")]
    public DateOnly? StartDate { get; set; }

    [Range(1, 60, ErrorMessage = "מספר המפגשים חייב להיות בין 1 ל-60")]
    public int SessionsCount { get; set; }

    [Required(ErrorMessage = "שעת ההתחלה היא שדה חובה")]
    public TimeOnly? StartTime { get; set; }

    [Range(15, 240, ErrorMessage = "משך המפגש חייב להיות בין 15 ל-240 דקות")]
    public int DurationMinutes { get; set; }

    [Range(3, 18, ErrorMessage = "הגיל המינימלי חייב להיות בין 3 ל-18")]
    public int MinAge { get; set; }

    [Range(3, 18, ErrorMessage = "הגיל המקסימלי חייב להיות בין 3 ל-18")]
    public int MaxAge { get; set; }

    [Range(0, 10000, ErrorMessage = "המחיר חייב להיות בין 0 ל-10,000")]
    public decimal Price { get; set; }

    [Range(1, 100, ErrorMessage = "מספר המקומות חייב להיות בין 1 ל-100")]
    public int Capacity { get; set; }
}
