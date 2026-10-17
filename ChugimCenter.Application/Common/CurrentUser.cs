using ChugimCenter.Domain.Enums;

namespace ChugimCenter.Application.Common;

/// <summary>
/// המשתמשת המחוברת, כפי שנקראה מתוך ה-token.
/// מועברת ל-Services כדי לבדוק הרשאות ברמת הנתונים (למשל "האם זו הקבוצה שלך").
/// </summary>
public record CurrentUser(int Id, UserRole Role)
{
    public bool IsAdmin => Role == UserRole.Admin;
}
