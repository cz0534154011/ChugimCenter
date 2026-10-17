using System.ComponentModel.DataAnnotations;

namespace ChugimCenter.Application.DTOs.Auth;

/// <summary>הרשמה עצמית של משתתפת. החשבון הוא של הילדה, עם פרטי ההורה.</summary>
public class RegisterDto : AccountRequestDto
{
    [Required(ErrorMessage = "תאריך הלידה הוא שדה חובה")]
    public DateOnly? BirthDate { get; set; }

    [Required(ErrorMessage = "שם ההורה הוא שדה חובה")]
    [MaxLength(60, ErrorMessage = "שם ההורה יכול להכיל עד 60 תווים")]
    public string ParentName { get; set; } = string.Empty;

    [Required(ErrorMessage = "טלפון ההורה הוא שדה חובה")]
    [RegularExpression(@"^0\d{1,2}-?\d{7}$", ErrorMessage = "מספר הטלפון אינו תקין")]
    public string ParentPhone { get; set; } = string.Empty;
}
