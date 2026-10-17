using System.ComponentModel.DataAnnotations;

namespace ChugimCenter.Application.DTOs.Auth;

public class LoginDto
{
    [Required(ErrorMessage = "האימייל הוא שדה חובה")]
    [EmailAddress(ErrorMessage = "כתובת האימייל אינה תקינה")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "הסיסמה היא שדה חובה")]
    public string Password { get; set; } = string.Empty;
}
