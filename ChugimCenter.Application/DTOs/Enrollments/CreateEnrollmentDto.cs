using System.ComponentModel.DataAnnotations;

namespace ChugimCenter.Application.DTOs.Enrollments;

/// <summary>רישום לחוג. המשתתפת נלקחת מה-token, ולא מהבקשה.</summary>
public class CreateEnrollmentDto
{
    [Range(1, int.MaxValue, ErrorMessage = "יש לבחור קבוצה")]
    public int CourseGroupId { get; set; }
}
