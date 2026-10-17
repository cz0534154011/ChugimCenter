using ChugimCenter.Domain.Enums;

namespace ChugimCenter.Application.DTOs.Users;

/// <summary>פרטי משתמשת שחוזרים מה-API. בלי סיסמה.</summary>
public class UserDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? Phone { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? ParentName { get; set; }
    public string? ParentPhone { get; set; }
    public DateTime CreatedAt { get; set; }
}
