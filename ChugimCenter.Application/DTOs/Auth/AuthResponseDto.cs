using ChugimCenter.Application.DTOs.Users;

namespace ChugimCenter.Application.DTOs.Auth;

/// <summary>תשובה להרשמה ולהתחברות: ה-token ופרטי המשתמשת.</summary>
public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;
}
