namespace ChugimCenter.Infrastructure.Security;

/// <summary>
/// הגדרות ה-JWT. נטענות מהקונפיגורציה דרך IOptions (בשלב 6).
/// המפתח (Key) לא כתוב בקוד ולא ב-appsettings: בפיתוח הוא נשמר ב-User Secrets.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;

    // מפתח החתימה. ל-HmacSha256 נדרשים לפחות 32 תווים
    public string Key { get; set; } = string.Empty;

    public int ExpiryMinutes { get; set; } = 120;
}
