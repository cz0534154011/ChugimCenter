using ChugimCenter.Domain.Entities;

namespace ChugimCenter.Application.Interfaces.Security;

/// <summary>token שנוצר, יחד עם מועד התפוגה שלו (ב-UTC).</summary>
public record GeneratedToken(string Token, DateTime ExpiresAt);

/// <summary>
/// יצירת token למשתמשת שהתחברה.
/// Application לא מכירה את ספריית ה-JWT; המימוש יושב ב-Infrastructure.
/// </summary>
public interface ITokenGenerator
{
    GeneratedToken Generate(User user);
}
