using System.ComponentModel.DataAnnotations;

namespace ChugimCenter.Application.DTOs.Auth;

/// <summary>שדות משותפים ליצירת חשבון: הרשמת משתתפת ויצירת מדריכה.</summary>
public abstract class AccountRequestDto
{
    [Required(ErrorMessage = "השם המלא הוא שדה חובה")]
    [MaxLength(60, ErrorMessage = "השם המלא יכול להכיל עד 60 תווים")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "האימייל הוא שדה חובה")]
    [EmailAddress(ErrorMessage = "כתובת האימייל אינה תקינה")]
    [MaxLength(100, ErrorMessage = "האימייל יכול להכיל עד 100 תווים")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "הסיסמה היא שדה חובה")]
    [MinLength(8, ErrorMessage = "הסיסמה חייבת להכיל לפחות 8 תווים")]
    [MaxLength(64, ErrorMessage = "הסיסמה יכולה להכיל עד 64 תווים")]
    public string Password { get; set; } = string.Empty;
}
